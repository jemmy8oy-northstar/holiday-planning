using HolidayPlanning.Abstractions.DomainModels;

namespace HolidayPlanning.Abstractions.Services;

public interface ITripService
{
    Task<IDomainTrip> CreateTripAsync(string name);
    Task<IDomainTrip?> GetTripAsync(Guid tripId);
    Task<IReadOnlyList<IDomainTripMember>> GetTripMembersAsync(Guid tripId);

    /// <summary>Adds a member to a trip. Returns null when the trip does not exist.</summary>
    Task<IDomainTripMember?> AddMemberAsync(Guid tripId, IDomainTripMember member);
}
