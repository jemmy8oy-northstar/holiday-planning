using HolidayPlanning.Abstractions.DomainModels;
using HolidayPlanning.DataModels.Models;

namespace HolidayPlanning.DomainModels.Models;

public class DomainStatus : Status, IDomainStatus
{
    public string GetFriendlyStatus()
    {
        return $"System is running version {Version} (Updated: {LastUpdated:g})";
    }
}
