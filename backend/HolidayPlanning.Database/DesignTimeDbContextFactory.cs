using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HolidayPlanning.Database;

/// <summary>
/// Used only by the dotnet-ef CLI to create migrations without a running app
/// or a configured connection string. Never used at runtime.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(
                "Host=localhost;Database=holidayplanning;Username=postgres;Password=postgres",
                b => b.MigrationsAssembly("HolidayPlanning.Database"))
            .Options;

        return new AppDbContext(options);
    }
}
