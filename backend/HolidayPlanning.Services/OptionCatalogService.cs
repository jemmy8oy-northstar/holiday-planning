using HolidayPlanning.Abstractions.DomainModels;
using HolidayPlanning.Abstractions.Services;
using HolidayPlanning.DomainModels.Models;

namespace HolidayPlanning.Services;

/// <summary>
/// MVP option catalog: a static, in-code set of realistic destination bundles
/// (dummy data — exactly what the MVP is asked to prove the concept with, D6).
/// Real flight/hotel data sources will later implement this same interface, so
/// nothing downstream knows the catalog is hard-coded. Options are ephemeral and
/// never persisted. Costs are per-person GBP from a single implied origin (D8).
/// </summary>
public class OptionCatalogService : IOptionCatalogService
{
    private static readonly IReadOnlyList<IDomainHolidayOption> Catalog = new List<IDomainHolidayOption>
    {
        Option("crete-beach-week", "Crete Beach Week", "Greece",
            "Golden beaches, tavernas and lazy afternoons in the Aegean sun.",
            720m, 4.0, 7,
            new() { ["beach"] = 1.0, ["food"] = 0.8, ["relaxation"] = 0.9, ["culture"] = 0.5, ["nightlife"] = 0.4 }),

        Option("lisbon-city-break", "Lisbon City Break", "Portugal",
            "Tiled streets, miradouros, pastéis de nata and a buzzing bar scene.",
            560m, 2.5, 4,
            new() { ["city"] = 1.0, ["culture"] = 0.9, ["food"] = 0.9, ["nightlife"] = 0.8, ["beach"] = 0.3 }),

        Option("ibiza-party-trip", "Ibiza Party Trip", "Spain",
            "World-famous clubs, beach parties and sunrise afterparties.",
            890m, 2.5, 5,
            new() { ["nightlife"] = 1.0, ["beach"] = 0.7, ["relaxation"] = 0.3, ["food"] = 0.5 }),

        Option("dolomites-hiking", "Dolomites Hiking", "Italy",
            "Alpine trails, via ferrata and rifugio dinners under the peaks.",
            810m, 3.0, 6,
            new() { ["nature"] = 1.0, ["adventure"] = 0.9, ["relaxation"] = 0.4, ["food"] = 0.6, ["wellness"] = 0.5 }),

        Option("marrakech-riad", "Marrakech Riad", "Morocco",
            "Souks, spice, a courtyard riad and day trips to the Atlas foothills.",
            640m, 3.5, 5,
            new() { ["culture"] = 1.0, ["food"] = 0.8, ["city"] = 0.7, ["adventure"] = 0.6, ["relaxation"] = 0.5 }),

        Option("algarve-family", "Algarve Family Resort", "Portugal",
            "Calm coves, a pool complex and easy days for all ages.",
            680m, 3.0, 7,
            new() { ["beach"] = 0.9, ["family"] = 1.0, ["relaxation"] = 0.9, ["food"] = 0.6, ["nightlife"] = 0.2 }),

        Option("iceland-ring-road", "Iceland Ring Road", "Iceland",
            "Waterfalls, glaciers, hot springs and the aurora if you're lucky.",
            1180m, 3.0, 6,
            new() { ["nature"] = 1.0, ["adventure"] = 0.9, ["wellness"] = 0.7, ["relaxation"] = 0.5 }),

        Option("amalfi-coast", "Amalfi Coast", "Italy",
            "Cliffside villages, limoncello, long lunches and boat days.",
            980m, 3.0, 6,
            new() { ["relaxation"] = 0.8, ["food"] = 1.0, ["culture"] = 0.7, ["beach"] = 0.6, ["nightlife"] = 0.4 }),

        Option("barcelona-weekend", "Barcelona Weekend", "Spain",
            "Gaudí, tapas crawls, city beach and a late-night rhythm.",
            520m, 2.0, 3,
            new() { ["city"] = 1.0, ["culture"] = 0.8, ["food"] = 0.9, ["nightlife"] = 0.8, ["beach"] = 0.5 }),

        Option("bali-wellness", "Bali Wellness Retreat", "Indonesia",
            "Rice-terrace yoga, spa days, surf and warm evenings.",
            1350m, 16.0, 10,
            new() { ["wellness"] = 1.0, ["relaxation"] = 1.0, ["nature"] = 0.7, ["beach"] = 0.8, ["adventure"] = 0.5 }),
    };

    public Task<IReadOnlyList<IDomainHolidayOption>> GetOptionsAsync() => Task.FromResult(Catalog);

    private static DomainHolidayOption Option(
        string slug, string name, string country, string description,
        decimal costPerPersonGbp, double travelHours, int nights,
        Dictionary<string, double> vibeIntensities) => new()
    {
        Slug = slug,
        Name = name,
        Country = country,
        Description = description,
        CostPerPersonGbp = costPerPersonGbp,
        TravelHours = travelHours,
        Nights = nights,
        VibeIntensities = vibeIntensities
    };
}
