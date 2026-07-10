namespace HolidayPlanning.EntityModels;

public class TripMemberEntity
{
    public Guid Id { get; set; }
    public Guid TripId { get; set; }
    public TripEntity? Trip { get; set; }
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Open vibe vocabulary → weight 0..1. Stored as jsonb.</summary>
    public Dictionary<string, double> VibeWeights { get; set; } = new();

    /// <summary>Hard constraints. Stored as jsonb.</summary>
    public List<DealbreakerRecord> Dealbreakers { get; set; } = new();
}
