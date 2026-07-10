using HolidayPlanning.Abstractions.DomainModels;

namespace HolidayPlanning.Abstractions.Services;

/// <summary>
/// Source of candidate holiday options. MVP: a static dummy catalog. Later:
/// real flight/hotel data sources behind this same interface (docs/DESIGN.md §2 D6).
/// </summary>
public interface IOptionCatalogService
{
    Task<IReadOnlyList<IDomainHolidayOption>> GetOptionsAsync();
}
