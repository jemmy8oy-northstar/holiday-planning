using HolidayPlanning.Abstractions.DomainModels;
using HolidayPlanning.Abstractions.Services;
using HolidayPlanning.Database;
using HolidayPlanning.DataModels.Models;
using HolidayPlanning.DomainModels.Models;
using HolidayPlanning.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace HolidayPlanning.Services;

/// <summary>
/// EF persistence boundary for trips and members — the single external-I/O seam
/// that unit and in-process integration tests mock (docs/specs/testing-strategy.md).
/// Mapping between entity and domain shapes is explicit and kept local to this
/// boundary; the jsonb value conversions live on the DbContext.
/// </summary>
public class TripStore(AppDbContext db) : ITripStore
{
    public async Task<IDomainTrip> CreateTripAsync(IDomainTrip trip)
    {
        var entity = new TripEntity
        {
            Id = trip.Id == Guid.Empty ? Guid.NewGuid() : trip.Id,
            Name = trip.Name,
            CreatedAtUtc = trip.CreatedAtUtc
        };

        db.Trips.Add(entity);
        await db.SaveChangesAsync();
        return ToDomain(entity);
    }

    public async Task<IDomainTrip?> GetTripAsync(Guid tripId)
    {
        var entity = await db.Trips.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tripId);
        return entity is null ? null : ToDomain(entity);
    }

    public async Task<IReadOnlyList<IDomainTripMember>> GetTripMembersAsync(Guid tripId)
    {
        var members = await db.TripMembers.AsNoTracking()
            .Where(m => m.TripId == tripId)
            .ToListAsync();

        return members.Select(m => (IDomainTripMember)ToDomain(m)).ToList();
    }

    public async Task<IDomainTripMember> AddMemberAsync(IDomainTripMember member)
    {
        var entity = ToEntity(member);
        db.TripMembers.Add(entity);
        await db.SaveChangesAsync();
        return ToDomain(entity);
    }

    private static DomainTrip ToDomain(TripEntity e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        CreatedAtUtc = e.CreatedAtUtc
    };

    private static DomainTripMember ToDomain(TripMemberEntity e) => new()
    {
        Id = e.Id,
        TripId = e.TripId,
        DisplayName = e.DisplayName,
        VibeWeights = new Dictionary<string, double>(e.VibeWeights),
        Dealbreakers = e.Dealbreakers
            .Select(d => new Dealbreaker { Type = d.Type, Threshold = d.Threshold, Vibe = d.Vibe })
            .ToList()
    };

    private static TripMemberEntity ToEntity(IDomainTripMember m) => new()
    {
        Id = m.Id == Guid.Empty ? Guid.NewGuid() : m.Id,
        TripId = m.TripId,
        DisplayName = m.DisplayName,
        VibeWeights = new Dictionary<string, double>(m.VibeWeights),
        Dealbreakers = m.Dealbreakers
            .Select(d => new DealbreakerRecord { Type = d.Type, Threshold = d.Threshold, Vibe = d.Vibe })
            .ToList()
    };
}
