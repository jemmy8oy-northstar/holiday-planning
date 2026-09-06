namespace HolidayPlanning.Abstractions.DataModels;

/// <summary>
/// One option scored against the whole group. Blocked options are returned and
/// explained, never silently dropped (docs/DESIGN.md §2 D4).
/// </summary>
public interface IOptionRecommendation
{
    IHolidayOption Option { get; }

    /// <summary>Fairness-aware group score 0..1 (docs/DESIGN.md §5).</summary>
    double GroupScore { get; set; }

    /// <summary>True when at least one member's dealbreaker vetoes this option.</summary>
    bool IsBlocked { get; set; }

    IReadOnlyList<IMemberFit> MemberFits { get; }
}
