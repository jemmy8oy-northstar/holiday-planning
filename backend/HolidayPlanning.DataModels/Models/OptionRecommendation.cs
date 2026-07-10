using HolidayPlanning.Abstractions.DataModels;

namespace HolidayPlanning.DataModels.Models;

public class OptionRecommendation : IOptionRecommendation
{
    public HolidayOption Option { get; set; } = new();
    public double GroupScore { get; set; }
    public bool IsBlocked { get; set; }
    public List<MemberFit> MemberFits { get; set; } = new();

    IHolidayOption IOptionRecommendation.Option => Option;
    IReadOnlyList<IMemberFit> IOptionRecommendation.MemberFits => MemberFits;
}
