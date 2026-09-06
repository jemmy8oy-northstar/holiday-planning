namespace HolidayPlanning.DataModels.Models;

/// <summary>
/// Known dealbreaker type keys (docs/DESIGN.md §4). Unknown types are ignored
/// by the scoring engine, so the set is open for extension.
/// </summary>
public static class DealbreakerTypes
{
    /// <summary>Vetoes options costing more than Threshold GBP per person.</summary>
    public const string BudgetPerPersonAbove = "budget_per_person_above";

    /// <summary>Vetoes options with one-way travel longer than Threshold hours.</summary>
    public const string TravelHoursAbove = "travel_hours_above";

    /// <summary>Vetoes options where the Vibe's intensity is below 0.5.</summary>
    public const string MustHaveVibe = "must_have_vibe";
}
