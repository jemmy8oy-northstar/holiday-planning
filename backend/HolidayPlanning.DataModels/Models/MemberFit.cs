using HolidayPlanning.Abstractions.DataModels;

namespace HolidayPlanning.DataModels.Models;

public class MemberFit : IMemberFit
{
    public string MemberName { get; set; } = string.Empty;
    public double Score { get; set; }
    public bool IsVetoed { get; set; }
    public List<string> Reasons { get; set; } = new();

    IReadOnlyList<string> IMemberFit.Reasons => Reasons;
}
