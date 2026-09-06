namespace HolidayPlanning.Abstractions.DataModels;

public interface ITripMember
{
    Guid Id { get; set; }
    Guid TripId { get; set; }
    string DisplayName { get; set; }

    /// <summary>Open vibe vocabulary → weight 0..1 (docs/DESIGN.md §2 D5).</summary>
    IReadOnlyDictionary<string, double> VibeWeights { get; }

    IReadOnlyList<IDealbreaker> Dealbreakers { get; }
}
