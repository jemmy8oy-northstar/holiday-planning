using HolidayPlanning.Abstractions.DomainModels;
using HolidayPlanning.Abstractions.Services;
using HolidayPlanning.Services;
using Moq;
using Xunit;

namespace HolidayPlanning.Tests;

/// <summary>
/// In-process integration golden test (docs/specs/testing-strategy.md): the real
/// <see cref="RecommendationService"/>, <see cref="OptionScoringService"/> and
/// <see cref="OptionCatalogService"/> wired together, with only the trip
/// persistence boundary mocked. It ranks the whole dummy catalog for a fixed
/// two-persona group and locks the ordering — so any future tuning of the engine
/// is a conscious, reviewed change (DESIGN §5).
/// </summary>
public class RecommendationRankingTests
{
    // Anna wants a relaxed beach holiday and won't spend over £1,000pp.
    private static IDomainTripMember Anna() => TestData.Member(
        "Anna",
        vibes: new() { ["beach"] = 1.0, ["relaxation"] = 0.8, ["food"] = 0.5 },
        TestData.Budget(1000));

    // Ben is after culture and city, and won't fly more than 5 hours.
    private static IDomainTripMember Ben() => TestData.Member(
        "Ben",
        vibes: new() { ["culture"] = 1.0, ["city"] = 0.8, ["food"] = 0.7, ["nightlife"] = 0.6 },
        TestData.TravelHours(5));

    private static readonly Guid TripId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    // Golden order — seeded from a verified run and sanity-checked against DESIGN §5.
    private static readonly string[] ExpectedOrder =
    {
        "barcelona-weekend",   // pleases both — Ben's city/culture/food, cheap enough for Anna
        "crete-beach-week",    // Anna's dream beach week, Ben lukewarm but not vetoed
        "amalfi-coast",        // food + relaxation for both, near Anna's budget ceiling
        "lisbon-city-break",
        "marrakech-riad",
        "algarve-family",
        "ibiza-party-trip",
        "dolomites-hiking",    // nature/adventure — neither weights it
        "iceland-ring-road",   // BLOCKED: £1,180 over Anna's £1,000
        "bali-wellness",       // BLOCKED: over Anna's budget and Ben's 5h flight limit
    };

    private static RecommendationService CreateSut()
    {
        var tripService = new Mock<ITripService>();
        tripService.Setup(s => s.GetTripAsync(TripId))
            .ReturnsAsync(TestData.Trip(TripId));
        tripService.Setup(s => s.GetTripMembersAsync(TripId))
            .ReturnsAsync(new List<IDomainTripMember> { Anna(), Ben() });

        return new RecommendationService(tripService.Object, new OptionCatalogService(), new OptionScoringService());
    }

    [Fact]
    public async Task Ranks_TheWholeCatalog_InAStableGoldenOrder()
    {
        var recs = await CreateSut().GetRecommendationsAsync(TripId);

        Assert.NotNull(recs);
        var order = recs!.Select(r => r.Option.Slug).ToArray();
        Assert.Equal(ExpectedOrder, order);
    }

    [Fact]
    public async Task Blocks_ExactlyTheOptionsAMemberVetoes()
    {
        var recs = await CreateSut().GetRecommendationsAsync(TripId);

        var blocked = recs!.Where(r => r.IsBlocked).Select(r => r.Option.Slug).OrderBy(s => s).ToArray();

        // iceland (£1,180 > Anna's £1,000) and bali (£1,350 and 16h > Ben's 5h).
        Assert.Equal(new[] { "bali-wellness", "iceland-ring-road" }, blocked);
    }

    [Fact]
    public async Task ViableOptions_RankAheadOfBlockedOnes()
    {
        var recs = await CreateSut().GetRecommendationsAsync(TripId);

        var firstBlocked = recs!.ToList().FindIndex(r => r.IsBlocked);
        var lastViable = recs!.ToList().FindLastIndex(r => !r.IsBlocked);
        Assert.True(lastViable < firstBlocked, "every viable option must rank above every blocked one");
    }

    [Fact]
    public async Task GroupScores_AreNonIncreasingWithinEachPartition()
    {
        var recs = await CreateSut().GetRecommendationsAsync(TripId);

        AssertNonIncreasing(recs!.Where(r => !r.IsBlocked).Select(r => r.GroupScore));
        AssertNonIncreasing(recs!.Where(r => r.IsBlocked).Select(r => r.GroupScore));
    }

    [Fact]
    public async Task EveryRecommendation_ExplainsEveryMember()
    {
        var recs = await CreateSut().GetRecommendationsAsync(TripId);

        Assert.All(recs!, rec =>
        {
            Assert.Equal(2, rec.MemberFits.Count);
            Assert.All(rec.MemberFits, fit => Assert.NotEmpty(fit.Reasons));
        });
    }

    private static void AssertNonIncreasing(IEnumerable<double> scores)
    {
        var list = scores.ToList();
        for (var i = 1; i < list.Count; i++)
            Assert.True(list[i] <= list[i - 1], $"scores must be non-increasing: {list[i - 1]} then {list[i]}");
    }
}
