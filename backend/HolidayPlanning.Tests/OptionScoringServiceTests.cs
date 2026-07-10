using HolidayPlanning.Abstractions.DomainModels;
using HolidayPlanning.Services;
using Xunit;

namespace HolidayPlanning.Tests;

/// <summary>
/// Pins the Compromise Engine (docs/DESIGN.md §5). These tests are the reason the
/// scoring constants are a conscious choice: change the blend and a test goes red.
/// </summary>
public class OptionScoringServiceTests
{
    private static OptionScoringService CreateSut() => new();

    private static IReadOnlyList<IDomainTripMember> Group(params IDomainTripMember[] members) => members;

    [Fact]
    public void OverBudget_VetoesForThatMember_AndBlocksOption()
    {
        var option = TestData.Option("pricey", costPerPersonGbp: 1200m);
        var anna = TestData.Member("Anna", dealbreakers: TestData.Budget(1000));

        var rec = CreateSut().ScoreOption(option, Group(anna));

        var fit = Assert.Single(rec.MemberFits);
        Assert.True(fit.IsVetoed);
        Assert.Equal(0d, fit.Score);
        Assert.True(rec.IsBlocked);
        Assert.Contains("budget", Assert.Single(fit.Reasons));
    }

    [Fact]
    public void OverTravelHours_Vetoes()
    {
        var option = TestData.Option("faraway", travelHours: 16.0);
        var ben = TestData.Member("Ben", dealbreakers: TestData.TravelHours(5));

        var rec = CreateSut().ScoreOption(option, Group(ben));

        Assert.True(Assert.Single(rec.MemberFits).IsVetoed);
        Assert.True(rec.IsBlocked);
    }

    [Fact]
    public void MustHaveVibe_BelowThreshold_Vetoes_ButAtThresholdDoesNot()
    {
        var weak = TestData.Option("weak-beach", vibes: new() { ["beach"] = 0.49 });
        var ok = TestData.Option("real-beach", vibes: new() { ["beach"] = 0.50 });
        var beachLover = TestData.Member("Cara", dealbreakers: TestData.MustHave("beach"));

        var sut = CreateSut();
        Assert.True(Assert.Single(sut.ScoreOption(weak, Group(beachLover)).MemberFits).IsVetoed);
        Assert.False(Assert.Single(sut.ScoreOption(ok, Group(beachLover)).MemberFits).IsVetoed);
    }

    [Fact]
    public void NoPreferencesAndNoBudget_ScoresNeutralHalf()
    {
        var option = TestData.Option("anywhere", vibes: new() { ["beach"] = 1.0 });
        var easygoing = TestData.Member("Dan");

        var rec = CreateSut().ScoreOption(option, Group(easygoing));

        // 0.7 * neutral(0.5) + 0.3 * neutral(0.5) == 0.5
        Assert.Equal(0.5d, Assert.Single(rec.MemberFits).Score, precision: 6);
    }

    [Fact]
    public void VibeMatch_IsWeightedAverageOfIntensities()
    {
        var option = TestData.Option("mixed", vibes: new() { ["beach"] = 0.8, ["food"] = 0.4, ["nightlife"] = 1.0 });
        var member = TestData.Member("Eve", vibes: new() { ["beach"] = 1.0, ["food"] = 0.5 });

        var rec = CreateSut().ScoreOption(option, Group(member));

        // vibeMatch = (1.0*0.8 + 0.5*0.4) / (1.0+0.5) = 0.6667 ; no budget => comfort 0.5
        // fit = 0.7*0.6667 + 0.3*0.5 = 0.61667
        Assert.Equal(0.61667d, Assert.Single(rec.MemberFits).Score, precision: 4);
    }

    [Fact]
    public void BudgetComfort_RewardsHeadroom()
    {
        var option = TestData.Option("cheap", costPerPersonGbp: 200m);
        var member = TestData.Member("Finn", dealbreakers: TestData.Budget(1000));

        var rec = CreateSut().ScoreOption(option, Group(member));

        // comfort = (1000-200)/1000 = 0.8 ; no vibes => vibeMatch 0.5
        // fit = 0.7*0.5 + 0.3*0.8 = 0.59
        Assert.Equal(0.59d, Assert.Single(rec.MemberFits).Score, precision: 6);
    }

    [Fact]
    public void GroupScore_BlendsMeanAndMin_AndVetoedMemberContributesZero()
    {
        var option = TestData.Option("split", costPerPersonGbp: 900m);
        var happy = TestData.Member("Gus");                                   // fit 0.5
        var priced = TestData.Member("Hana", dealbreakers: TestData.Budget(500)); // vetoed, fit 0

        var rec = CreateSut().ScoreOption(option, Group(happy, priced));

        // mean(0.5, 0) = 0.25 ; min = 0 ; group = 0.6*0.25 + 0.4*0 = 0.15
        Assert.Equal(0.15d, rec.GroupScore, precision: 6);
        Assert.True(rec.IsBlocked);
    }

    [Fact]
    public void MemberFit_NamesTopContributingVibes()
    {
        var option = TestData.Option("beachy", vibes: new() { ["beach"] = 1.0, ["food"] = 0.9, ["nightlife"] = 0.1 });
        var member = TestData.Member("Ivy", vibes: new() { ["beach"] = 1.0, ["food"] = 0.8, ["nightlife"] = 0.2 });

        var rec = CreateSut().ScoreOption(option, Group(member));

        var reason = Assert.Single(Assert.Single(rec.MemberFits).Reasons);
        Assert.Contains("beach", reason);
    }
}
