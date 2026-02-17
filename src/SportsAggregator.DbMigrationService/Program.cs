using Microsoft.EntityFrameworkCore;
using SportsAggregator.Infrastructure.Data;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<SportsDbContext>("sportsdb");

using var host = builder.Build();
await using var scope = host.Services.CreateAsyncScope();

var logger = scope.ServiceProvider
    .GetRequiredService<ILoggerFactory>()
    .CreateLogger("SportsAggregator.DbMigrationService");

var dbContext = scope.ServiceProvider
    .GetRequiredService<SportsDbContext>();

logger.LogInformation("Applying database migrations");
await dbContext.Database.MigrateAsync();
logger.LogInformation("Database migrations applied successfully");
