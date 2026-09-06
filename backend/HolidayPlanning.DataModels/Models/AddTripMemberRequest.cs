namespace HolidayPlanning.DataModels.Models;

public class AddTripMemberRequest
{
    public string DisplayName { get; set; } = string.Empty;
    public Dictionary<string, double> VibeWeights { get; set; } = new();
    public List<Dealbreaker> Dealbreakers { get; set; } = new();
}
