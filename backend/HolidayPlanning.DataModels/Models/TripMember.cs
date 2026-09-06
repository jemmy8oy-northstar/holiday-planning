using HolidayPlanning.Abstractions.DataModels;

namespace HolidayPlanning.DataModels.Models;

public class TripMember : ITripMember
{
    public Guid Id { get; set; }
    public Guid TripId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public Dictionary<string, double> VibeWeights { get; set; } = new();
    public List<Dealbreaker> Dealbreakers { get; set; } = new();

    IReadOnlyDictionary<string, double> ITripMember.VibeWeights => VibeWeights;
    IReadOnlyList<IDealbreaker> ITripMember.Dealbreakers => Dealbreakers;
}
