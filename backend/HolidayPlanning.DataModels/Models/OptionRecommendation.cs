using HolidayPlanning.Abstractions.DataModels;

namespace HolidayPlanning.DataModels.Models;

public class OptionRecommendation : IOptionRecommendation
{
    public required HolidayOption Option { get; set; }
    public double GroupScore { get; set; }
    public bool IsBlocked { get; set; }
    public required List<MemberFit> MemberFits { get; set; }

    IHolidayOption IOptionRecommendation.Option => Option;
    IReadOnlyList<IMemberFit> IOptionRecommendation.MemberFits => MemberFits;
}
