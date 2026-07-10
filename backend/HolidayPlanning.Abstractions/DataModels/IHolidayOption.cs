namespace HolidayPlanning.Abstractions.DataModels;

/// <summary>
/// A candidate holiday bundle from the option catalog. Options are ephemeral —
/// they are never persisted (docs/DESIGN.md §2 D6).
/// </summary>
public interface IHolidayOption
{
    /// <summary>Stable catalog identity, e.g. "crete-beach-week".</summary>
    string Slug { get; set; }

    string Name { get; set; }
    string Country { get; set; }
    string Description { get; set; }
    decimal CostPerPersonGbp { get; set; }
    double TravelHours { get; set; }
    int Nights { get; set; }

    /// <summary>Vibe key → intensity 0..1 describing what this option offers.</summary>
    IReadOnlyDictionary<string, double> VibeIntensities { get; }
}
