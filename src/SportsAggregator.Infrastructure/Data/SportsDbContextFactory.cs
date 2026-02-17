using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SportsAggregator.Infrastructure.Data;

public sealed class SportsDbContextFactory : IDesignTimeDbContextFactory<SportsDbContext>
{
    private const string DefaultConnectionString =
        "Host=localhost;Port=5432;Database=sportsdb;Username=postgres;Password=postgres";

    public SportsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SportsDbContext>();
        optionsBuilder.UseNpgsql(DefaultConnectionString);

        return new SportsDbContext(optionsBuilder.Options);
    }
}
