using HolidayPlanning.Abstractions.DomainModels;
using HolidayPlanning.DataModels.Models;

namespace HolidayPlanning.DomainModels.Models;

public class DomainHolidayOption : HolidayOption, IDomainHolidayOption
{
    public double IntensityOf(string vibe) =>
        VibeIntensities.TryGetValue(vibe, out var intensity) ? intensity : 0d;
}
