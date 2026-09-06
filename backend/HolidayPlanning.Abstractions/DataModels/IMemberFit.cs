namespace HolidayPlanning.Abstractions.DataModels;

/// <summary>
/// How well one option suits one member — always carries human-readable
/// reasons (docs/DESIGN.md §2 D2).
/// </summary>
public interface IMemberFit
{
    string MemberName { get; set; }

    /// <summary>Fit 0..1. Always 0 when vetoed.</summary>
    double Score { get; set; }

    /// <summary>True when one of this member's dealbreakers excludes the option.</summary>
    bool IsVetoed { get; set; }

    IReadOnlyList<string> Reasons { get; }
}
