using HolidayPlanning.Abstractions.DomainModels;

namespace HolidayPlanning.Abstractions.Services;

/// <summary>
/// Persistence boundary for trips. This is the external I/O seam that unit and
/// in-process integration tests mock (docs/specs/testing-strategy.md).
/// </summary>
public interface ITripStore
{
    Task<IDomainTrip> CreateTripAsync(IDomainTrip trip);
    Task<IDomainTrip?> GetTripAsync(Guid tripId);
    Task<IReadOnlyList<IDomainTripMember>> GetTripMembersAsync(Guid tripId);
    Task<IDomainTripMember> AddMemberAsync(IDomainTripMember member);
}
