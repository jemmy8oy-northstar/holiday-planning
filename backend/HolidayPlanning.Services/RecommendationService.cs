using HolidayPlanning.Abstractions.DomainModels;
using HolidayPlanning.Abstractions.Services;

namespace HolidayPlanning.Services;

/// <summary>
/// Orchestrates the Compromise Engine over a trip: load the group and the option
/// catalog, score every option, and rank them — viable options by group score,
/// then blocked options by group score. Blocked options are never dropped, so the
/// group can see what unlocks if someone relaxes a constraint (DESIGN §5, D4).
/// </summary>
public class RecommendationService(
    ITripService tripService,
    IOptionCatalogService catalog,
    IOptionScoringService scoring) : IRecommendationService
{
    public async Task<IReadOnlyList<IDomainOptionRecommendation>?> GetRecommendationsAsync(Guid tripId)
    {
        var trip = await tripService.GetTripAsync(tripId);
        if (trip is null) return null;

        var members = await tripService.GetTripMembersAsync(tripId);
        var options = await catalog.GetOptionsAsync();

        return options
            .Select(option => scoring.ScoreOption(option, members))
            .OrderBy(recommendation => recommendation.IsBlocked)     // viable (false) before blocked (true)
            .ThenByDescending(recommendation => recommendation.GroupScore)
            .ToList();
    }
}
