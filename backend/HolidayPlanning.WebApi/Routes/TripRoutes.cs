using HolidayPlanning.Abstractions.Services;
using HolidayPlanning.DataModels.Models;
using HolidayPlanning.DomainModels.Models;

namespace HolidayPlanning.WebApi.Routes;

/// <summary>
/// The MVP trip API (docs/DESIGN.md §9): create a trip, add members with their
/// preferences and dealbreakers, read a trip, and fetch ranked recommendations
/// with full per-member explanations. Routes project domain models to plain data
/// models so the public contract never leaks domain-only members.
/// </summary>
public static class TripRoutes
{
    public static RouteGroupBuilder MapTripRoutes(this RouteGroupBuilder parentGroup)
    {
        var group = parentGroup.MapGroup("/trips");

        group.MapPost("", async (CreateTripRequest request, ITripService tripService) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Results.BadRequest(new { error = "Trip name is required." });

            var trip = await tripService.CreateTripAsync(request.Name.Trim());
            var model = ToDataTrip(trip);
            return Results.Created($"/api/trips/{model.Id}", model);
        })
        .WithName("CreateTrip");

        group.MapGet("/{tripId:guid}", async (Guid tripId, ITripService tripService) =>
        {
            var trip = await tripService.GetTripAsync(tripId);
            if (trip is null) return Results.NotFound();

            var members = await tripService.GetTripMembersAsync(tripId);
            return Results.Ok(new TripDetailsResponse
            {
                Trip = ToDataTrip(trip),
                Members = members.Select(ToDataMember).ToList()
            });
        })
        .WithName("GetTrip");

        group.MapPost("/{tripId:guid}/members", async (
            Guid tripId, AddTripMemberRequest request, ITripService tripService) =>
        {
            if (string.IsNullOrWhiteSpace(request.DisplayName))
                return Results.BadRequest(new { error = "Member display name is required." });

            var member = new DomainTripMember
            {
                DisplayName = request.DisplayName.Trim(),
                VibeWeights = new Dictionary<string, double>(request.VibeWeights),
                Dealbreakers = request.Dealbreakers
                    .Select(d => new Dealbreaker { Type = d.Type, Threshold = d.Threshold, Vibe = d.Vibe })
                    .ToList()
            };

            var added = await tripService.AddMemberAsync(tripId, member);
            return added is null ? Results.NotFound() : Results.Ok(ToDataMember(added));
        })
        .WithName("AddTripMember");

        group.MapGet("/{tripId:guid}/recommendations", async (
            Guid tripId, IRecommendationService recommendationService) =>
        {
            var recommendations = await recommendationService.GetRecommendationsAsync(tripId);
            return recommendations is null
                ? Results.NotFound()
                : Results.Ok(recommendations.Select(ToDataRecommendation).ToList());
        })
        .WithName("GetTripRecommendations");

        return parentGroup;
    }

    private static Trip ToDataTrip(Abstractions.DomainModels.IDomainTrip trip) => new()
    {
        Id = trip.Id,
        Name = trip.Name,
        CreatedAtUtc = trip.CreatedAtUtc
    };

    private static TripMember ToDataMember(Abstractions.DomainModels.IDomainTripMember member) => new()
    {
        Id = member.Id,
        TripId = member.TripId,
        DisplayName = member.DisplayName,
        VibeWeights = new Dictionary<string, double>(member.VibeWeights),
        Dealbreakers = member.Dealbreakers
            .Select(d => new Dealbreaker { Type = d.Type, Threshold = d.Threshold, Vibe = d.Vibe })
            .ToList()
    };

    private static OptionRecommendation ToDataRecommendation(
        Abstractions.DomainModels.IDomainOptionRecommendation recommendation) => new()
    {
        Option = new HolidayOption
        {
            Slug = recommendation.Option.Slug,
            Name = recommendation.Option.Name,
            Country = recommendation.Option.Country,
            Description = recommendation.Option.Description,
            CostPerPersonGbp = recommendation.Option.CostPerPersonGbp,
            TravelHours = recommendation.Option.TravelHours,
            Nights = recommendation.Option.Nights,
            VibeIntensities = recommendation.Option.VibeIntensities.ToDictionary(kv => kv.Key, kv => kv.Value)
        },
        GroupScore = recommendation.GroupScore,
        IsBlocked = recommendation.IsBlocked,
        MemberFits = recommendation.MemberFits
            .Select(f => new MemberFit
            {
                MemberName = f.MemberName,
                Score = f.Score,
                IsVetoed = f.IsVetoed,
                Reasons = f.Reasons.ToList()
            })
            .ToList()
    };
}
