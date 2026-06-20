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
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, b => b.MigrationsAssembly("HolidayPlanning.Database")));

        services.AddAutoMapper(cfg => cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()));
        services.AddScoped<IStatusService, StatusService>();
    }
}
