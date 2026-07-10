namespace HolidayPlanning.EntityModels;

public class TripEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public List<TripMemberEntity> Members { get; set; } = new();
}
