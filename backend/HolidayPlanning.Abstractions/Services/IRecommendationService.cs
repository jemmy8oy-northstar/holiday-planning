using HolidayPlanning.Abstractions.DomainModels;

namespace HolidayPlanning.Abstractions.Services;

public interface IRecommendationService
{
    /// <summary>
    /// Ranked recommendations for a trip: viable options by group score, then
    /// blocked options with their veto explanations. Null when the trip does not exist.
    /// </summary>
    Task<IReadOnlyList<IDomainOptionRecommendation>?> GetRecommendationsAsync(Guid tripId);
}
