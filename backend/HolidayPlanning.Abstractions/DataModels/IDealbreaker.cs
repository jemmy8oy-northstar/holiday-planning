namespace HolidayPlanning.Abstractions.DataModels;

/// <summary>
/// A hard constraint held by one trip member. Dealbreakers veto options rather
/// than lowering their score — see docs/DESIGN.md §2 (D4).
/// </summary>
public interface IDealbreaker
{
    /// <summary>One of the known dealbreaker type keys (see DataModels DealbreakerTypes).</summary>
    string Type { get; set; }

    /// <summary>Numeric limit for threshold-style dealbreakers (budget GBP, travel hours).</summary>
    double Threshold { get; set; }

    /// <summary>Vibe key for vibe-style dealbreakers (e.g. must_have_vibe "beach").</summary>
    string? Vibe { get; set; }
}
