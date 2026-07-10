namespace HolidayPlanning.Abstractions.DomainModels;

using HolidayPlanning.Abstractions.DataModels;

public interface IDomainTripMember : ITripMember
{
    /// <summary>True when the member has stated at least one vibe preference.</summary>
    bool HasVibePreferences { get; }

    /// <summary>The member's weight for a vibe, 0 when unstated.</summary>
    double WeightFor(string vibe);
}
