namespace HolidayPlanning.EntityModels;

/// <summary>JSON-serialised inside TripMemberEntity.Dealbreakers (jsonb).</summary>
public class DealbreakerRecord
{
    public string Type { get; set; } = string.Empty;
    public double Threshold { get; set; }
    public string? Vibe { get; set; }
}
