using HolidayPlanning.Abstractions.DomainModels;
using HolidayPlanning.Abstractions.Services;
using HolidayPlanning.DataModels.Models;
using HolidayPlanning.Services;
using Xunit;

namespace HolidayPlanning.Tests;

/// <summary>
/// Trip lifecycle through the real <see cref="TripService"/> over an in-memory
/// <see cref="ITripStore"/> (the persistence seam): create a trip, add members
/// with vibes and dealbreakers, read the trip back with its members.
/// </summary>
public class TripServiceTests
{
    private sealed class InMemoryTripStore : ITripStore
    {
        private readonly Dictionary<Guid, IDomainTrip> _trips = new();
        private readonly List<IDomainTripMember> _members = new();

        public Task<IDomainTrip> CreateTripAsync(IDomainTrip trip)
        {
            _trips[trip.Id] = trip;
            return Task.FromResult(trip);
        }

        public Task<IDomainTrip?> GetTripAsync(Guid tripId) =>
            Task.FromResult(_trips.TryGetValue(tripId, out var t) ? t : null);

        public Task<IReadOnlyList<IDomainTripMember>> GetTripMembersAsync(Guid tripId) =>
            Task.FromResult<IReadOnlyList<IDomainTripMember>>(
                _members.Where(m => m.TripId == tripId).ToList());

        public Task<IDomainTripMember> AddMemberAsync(IDomainTripMember member)
        {
            _members.Add(member);
            return Task.FromResult(member);
        }
    }

    private static TripService CreateSut() => new(new InMemoryTripStore());

    [Fact]
    public async Task CreateTrip_GivenAName_ReturnsATripWithThatNameAndServerManagedFields()
    {
        var before = DateTime.UtcNow;

        var trip = await CreateSut().CreateTripAsync("Summer 2027");

        Assert.Equal("Summer 2027", trip.Name);
        Assert.NotEqual(Guid.Empty, trip.Id);
        Assert.True(trip.CreatedAtUtc >= before);
    }

    [Fact]
    public async Task CreateTrip_Twice_GivesEachTripItsOwnId()
    {
        var sut = CreateSut();

        var a = await sut.CreateTripAsync("A");
        var b = await sut.CreateTripAsync("B");

        Assert.NotEqual(a.Id, b.Id);
        Assert.Equal("A", (await sut.GetTripAsync(a.Id))!.Name);
        Assert.Equal("B", (await sut.GetTripAsync(b.Id))!.Name);
    }

    [Fact]
    public async Task AddMember_ToExistingTrip_KeepsVibePreferencesAndDealbreakers()
    {
        var sut = CreateSut();
        var trip = await sut.CreateTripAsync("Trip");
        var member = TestData.Member(
            "Anna",
            new() { ["beach"] = 1.0, ["food"] = 0.5 },
            TestData.Budget(900), TestData.MustHave("beach"));

        var added = await sut.AddMemberAsync(trip.Id, member);

        Assert.NotNull(added);
        Assert.Equal(trip.Id, added!.TripId);
        Assert.NotEqual(Guid.Empty, added.Id);
        Assert.Equal("Anna", added.DisplayName);
        Assert.Equal(1.0, added.VibeWeights["beach"]);
        Assert.Equal(0.5, added.VibeWeights["food"]);
        Assert.Equal(2, added.Dealbreakers.Count);
        Assert.Contains(added.Dealbreakers,
            d => d.Type == DealbreakerTypes.BudgetPerPersonAbove && d.Threshold == 900);
        Assert.Contains(added.Dealbreakers,
            d => d.Type == DealbreakerTypes.MustHaveVibe && d.Vibe == "beach");
    }

    [Fact]
    public async Task AddMember_ToUnknownTrip_ReturnsNull()
    {
        var added = await CreateSut().AddMemberAsync(Guid.NewGuid(), TestData.Member("Ghost"));

        Assert.Null(added);
    }

    [Fact]
    public async Task ReadTrip_AfterAddingMembers_ShowsTheTripAndItsMembersOnly()
    {
        var sut = CreateSut();
        var trip = await sut.CreateTripAsync("Reading trip");
        var other = await sut.CreateTripAsync("Other trip");
        await sut.AddMemberAsync(trip.Id, TestData.Member("Anna", new() { ["beach"] = 1.0 }, TestData.Budget(1000)));
        await sut.AddMemberAsync(trip.Id, TestData.Member("Ben", new() { ["culture"] = 0.9 }));
        await sut.AddMemberAsync(other.Id, TestData.Member("Cara"));

        var read = await sut.GetTripAsync(trip.Id);
        var members = await sut.GetTripMembersAsync(trip.Id);

        Assert.Equal("Reading trip", read!.Name);
        Assert.Equal(new[] { "Anna", "Ben" }, members.Select(m => m.DisplayName).OrderBy(n => n));
        var anna = members.Single(m => m.DisplayName == "Anna");
        Assert.Equal(1.0, anna.VibeWeights["beach"]);
        Assert.Equal(1000, anna.Dealbreakers.Single().Threshold);
    }

    [Fact]
    public async Task ReadTrip_Unknown_ReturnsNullAndNoMembers()
    {
        var sut = CreateSut();
        var id = Guid.NewGuid();

        Assert.Null(await sut.GetTripAsync(id));
        Assert.Empty(await sut.GetTripMembersAsync(id));
    }
}
