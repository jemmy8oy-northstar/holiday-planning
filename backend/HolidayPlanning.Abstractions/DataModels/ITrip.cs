namespace HolidayPlanning.Abstractions.DataModels;

public interface ITrip
{
    Guid Id { get; set; }
    string Name { get; set; }
    DateTime CreatedAtUtc { get; set; }
}
