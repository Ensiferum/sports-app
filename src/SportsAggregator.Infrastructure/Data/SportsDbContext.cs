using Microsoft.EntityFrameworkCore;
using SportsAggregator.Domain.Entities;

namespace SportsAggregator.Infrastructure.Data;

public sealed class SportsDbContext(DbContextOptions<SportsDbContext> options) : DbContext(options)
{
    public DbSet<Game> Games => Set<Game>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SportsDbContext).Assembly);
    }
}
