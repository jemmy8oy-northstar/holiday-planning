namespace HolidayPlanning.Abstractions.DomainModels;

using HolidayPlanning.Abstractions.DataModels;

public interface IDomainHolidayOption : IHolidayOption
{
    /// <summary>The option's intensity for a vibe, 0 when it offers none of it.</summary>
    double IntensityOf(string vibe);
}
