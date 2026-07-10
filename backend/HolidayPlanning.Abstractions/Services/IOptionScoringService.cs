using HolidayPlanning.Abstractions.DomainModels;

namespace HolidayPlanning.Abstractions.Services;

/// <summary>
/// The Compromise Engine: scores one option against every member of a group.
/// Pure and deterministic — no I/O, no LLM (docs/DESIGN.md §2 D1, §5).
/// </summary>
public interface IOptionScoringService
{
    IDomainOptionRecommendation ScoreOption(
        IDomainHolidayOption option,
        IReadOnlyList<IDomainTripMember> members);
}
