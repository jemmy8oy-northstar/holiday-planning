namespace HolidayPlanning.DataModels.Models;

/// <summary>
/// Assembled by the GetTrip route from two service calls (trip + members) —
/// a genuine *Response per the naming rules in docs/specs/backend-architecture.md.
/// </summary>
public class TripDetailsResponse
{
    public Trip Trip { get; set; } = new();
    public List<TripMember> Members { get; set; } = new();
}
