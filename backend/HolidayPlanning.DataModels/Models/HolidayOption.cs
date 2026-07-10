using HolidayPlanning.Abstractions.DataModels;

namespace HolidayPlanning.DataModels.Models;

public class HolidayOption : IHolidayOption
{
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal CostPerPersonGbp { get; set; }
    public double TravelHours { get; set; }
    public int Nights { get; set; }
    public Dictionary<string, double> VibeIntensities { get; set; } = new();

    IReadOnlyDictionary<string, double> IHolidayOption.VibeIntensities => VibeIntensities;
}
