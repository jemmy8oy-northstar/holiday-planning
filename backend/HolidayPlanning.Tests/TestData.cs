using HolidayPlanning.DataModels.Models;
using HolidayPlanning.DomainModels.Models;

namespace HolidayPlanning.Tests;

/// <summary>Small builders so tests read as intent, not construction.</summary>
internal static class TestData
{
    public static DomainTrip Trip(Guid id, string name = "Test Trip") => new()
    {
        Id = id,
        Name = name,
        CreatedAtUtc = DateTime.UtcNow
    };

    public static DomainTripMember Member(
        string name,
        Dictionary<string, double>? vibes = null,
        params Dealbreaker[] dealbreakers) => new()
    {
        Id = Guid.NewGuid(),
        DisplayName = name,
        VibeWeights = vibes ?? new Dictionary<string, double>(),
        Dealbreakers = dealbreakers.ToList()
    };

    public static DomainHolidayOption Option(
        string slug,
        decimal costPerPersonGbp = 500m,
        double travelHours = 3.0,
        Dictionary<string, double>? vibes = null) => new()
    {
        Slug = slug,
        Name = slug,
        Country = "Testland",
        Description = slug,
        CostPerPersonGbp = costPerPersonGbp,
        TravelHours = travelHours,
        Nights = 7,
        VibeIntensities = vibes ?? new Dictionary<string, double>()
    };

    public static Dealbreaker Budget(double gbp) =>
        new() { Type = DealbreakerTypes.BudgetPerPersonAbove, Threshold = gbp };

    public static Dealbreaker TravelHours(double hours) =>
        new() { Type = DealbreakerTypes.TravelHoursAbove, Threshold = hours };

    public static Dealbreaker MustHave(string vibe) =>
        new() { Type = DealbreakerTypes.MustHaveVibe, Vibe = vibe };
}
