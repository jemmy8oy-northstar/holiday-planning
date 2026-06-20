using Microsoft.EntityFrameworkCore;

namespace HolidayPlanning.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // Add your DbSets here
    // public DbSet<MyEntity> MyEntities => Set<MyEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
