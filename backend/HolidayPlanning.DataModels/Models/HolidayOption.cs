using HolidayPlanning.Abstractions.DataModels;

namespace HolidayPlanning.DataModels.Models;

public class HolidayOption : IHolidayOption
{
    public required string Slug { get; set; }
    public required string Name { get; set; }
    public required string Country { get; set; }
    public required string Description { get; set; }
    public decimal CostPerPersonGbp { get; set; }
    public double TravelHours { get; set; }
    public int Nights { get; set; }
    public required Dictionary<string, double> VibeIntensities { get; set; }

    IReadOnlyDictionary<string, double> IHolidayOption.VibeIntensities => VibeIntensities;
}
