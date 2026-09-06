using HolidayPlanning.Abstractions.DataModels;

namespace HolidayPlanning.DataModels.Models;

public class Dealbreaker : IDealbreaker
{
    public string Type { get; set; } = string.Empty;
    public double Threshold { get; set; }
    public string? Vibe { get; set; }
}
