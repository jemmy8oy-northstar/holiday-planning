using HolidayPlanning.Abstractions.DomainModels;
using HolidayPlanning.DataModels.Models;

namespace HolidayPlanning.DomainModels.Models;

public class DomainTripMember : TripMember, IDomainTripMember
{
    public bool HasVibePreferences => VibeWeights.Count > 0;

    public double WeightFor(string vibe) =>
        VibeWeights.TryGetValue(vibe, out var weight) ? weight : 0d;
}
