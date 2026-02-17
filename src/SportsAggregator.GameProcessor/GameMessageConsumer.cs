using MassTransit;
using Microsoft.EntityFrameworkCore;
using SportsAggregator.Domain.Contracts;
using SportsAggregator.Domain.Entities;
using SportsAggregator.Domain.Services;
using SportsAggregator.Infrastructure.Data;

namespace SportsAggregator.GameProcessor;

public sealed class GameMessageConsumer(
    SportsDbContext dbContext,
    ILogger<GameMessageConsumer> logger) : IConsumer<IngestedGameMessage>
{
    private static readonly TimeSpan DuplicateWindow = TimeSpan.FromHours(2);

    public async Task Consume(ConsumeContext<IngestedGameMessage> context)
    {
        var message = context.Message;
        var scheduledAtUtc = EnsureUtc(message.ScheduledAtUtc);

        var matchKey = MatchKeyGenerator.Generate(
            message.SportType,
            message.CompetitionName,
            message.HomeTeam,
            message.AwayTeam);

        var windowStartUtc = scheduledAtUtc - DuplicateWindow;
        var windowEndUtc = scheduledAtUtc + DuplicateWindow;

        var isDuplicate = await dbContext.Games
            .AsNoTracking()
            .AnyAsync(
                game => game.MatchKey == matchKey
                    && game.ScheduledAtUtc >= windowStartUtc
                    && game.ScheduledAtUtc <= windowEndUtc,
                context.CancellationToken);

        if (isDuplicate)
        {
            logger.LogInformation("Duplicate game detected by absolute 2-hour window");
            return;
        }

        try
        {
            var game = new Game
            {
                Id = Guid.CreateVersion7(),
                ScheduledAtUtc = scheduledAtUtc,
                SportType = MatchKeyGenerator.Normalize(message.SportType),
                CompetitionName = message.CompetitionName.Trim(),
                HomeTeam = message.HomeTeam.Trim(),
                AwayTeam = message.AwayTeam.Trim(),
                MatchKey = matchKey,
                CreatedAtUtc = DateTime.UtcNow
            };

            dbContext.Games.Add(game);
            await dbContext.SaveChangesAsync(context.CancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process game message");
            throw;
        }
    }

    private static DateTime EnsureUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
    }
}
