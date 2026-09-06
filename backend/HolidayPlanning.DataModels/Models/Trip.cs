using HolidayPlanning.Abstractions.DataModels;

namespace HolidayPlanning.DataModels.Models;

public class Trip : ITrip
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
