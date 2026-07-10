using HolidayPlanning.Abstractions.DomainModels;
using HolidayPlanning.Abstractions.Services;
using HolidayPlanning.DomainModels.Models;

namespace HolidayPlanning.Services;

/// <summary>
/// Trip lifecycle orchestration over the persistence boundary (<see cref="ITripStore"/>).
/// Owns server-managed fields (Id, CreatedAtUtc) so route models never carry them.
/// </summary>
public class TripService(ITripStore store) : ITripService
{
    public Task<IDomainTrip> CreateTripAsync(string name) =>
        store.CreateTripAsync(new DomainTrip
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedAtUtc = DateTime.UtcNow
        });

    public Task<IDomainTrip?> GetTripAsync(Guid tripId) => store.GetTripAsync(tripId);

    public Task<IReadOnlyList<IDomainTripMember>> GetTripMembersAsync(Guid tripId) =>
        store.GetTripMembersAsync(tripId);

    public async Task<IDomainTripMember?> AddMemberAsync(Guid tripId, IDomainTripMember member)
    {
        var trip = await store.GetTripAsync(tripId);
        if (trip is null) return null;

        member.TripId = tripId;
        member.Id = Guid.NewGuid();
        return await store.AddMemberAsync(member);
    }
}
