namespace HolidayPlanning.Abstractions.DomainModels;

using HolidayPlanning.Abstractions.DataModels;

public interface IDomainStatus : IStatus
{
    string GetFriendlyStatus();
}
