using HolidayPlanning.Abstractions.Services;
using HolidayPlanning.Services;
using HolidayPlanning.Database;
using Microsoft.EntityFrameworkCore;

namespace HolidayPlanning.WebApi;

public static class ServiceRegistration
{
    public static void AddBackendServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Console.WriteLine("[WARNING] No database connection string configured — database features are disabled.");
        }
        else
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString, b => b.MigrationsAssembly("HolidayPlanning.Database")));
        }

        services.AddAutoMapper(cfg => cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()));
        services.AddScoped<IStatusService, StatusService>();

        // Holiday planning (docs/DESIGN.md). The scoring engine and dummy catalog
        // are pure/stateless → singletons; the trip services touch the DbContext.
        services.AddScoped<ITripStore, TripStore>();
        services.AddScoped<ITripService, TripService>();
        services.AddScoped<IRecommendationService, RecommendationService>();
        services.AddSingleton<IOptionCatalogService, OptionCatalogService>();
        services.AddSingleton<IOptionScoringService, OptionScoringService>();
    }
}
