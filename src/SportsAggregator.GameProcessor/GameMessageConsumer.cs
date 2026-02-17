using MassTransit;
using Npgsql;
using SportsAggregator.Domain.Contracts;
using SportsAggregator.Domain.Entities;
using SportsAggregator.Domain.Services;
using SportsAggregator.Infrastructure.Data;
using SportsAggregator.Infrastructure.Services;

namespace SportsAggregator.GameProcessor;

public sealed class GameMessageConsumer(
    SportsDbContext dbContext,
    IDeduplicationService deduplicationService,
    ILogger<GameMessageConsumer> logger) : IConsumer<IngestedGameMessage>
{
    private static readonly TimeSpan DeduplicationTtl = TimeSpan.FromHours(2);

    public async Task Consume(ConsumeContext<IngestedGameMessage> context)
    {
        var message = context.Message;

        var primaryFingerprint = FingerprintGenerator.Generate(
            message.SportType,
            message.CompetitionName,
            message.HomeTeam,
            message.AwayTeam,
            message.ScheduledAtUtc);

        var adjacentFingerprint = FingerprintGenerator.GenerateAdjacentBucket(
            message.SportType,
            message.CompetitionName,
            message.HomeTeam,
            message.AwayTeam,
            message.ScheduledAtUtc);

        var isDuplicate = await deduplicationService.IsDuplicateAsync(
            primaryFingerprint,
            adjacentFingerprint,
            context.CancellationToken);

        if (isDuplicate)
        {
            return;
        }

        try
        {
            var game = new Game
            {
                Id = Guid.CreateVersion7(),
                ScheduledAtUtc = message.ScheduledAtUtc,
                SportType = message.SportType.Trim().ToLowerInvariant(),
                CompetitionName = message.CompetitionName.Trim(),
                HomeTeam = message.HomeTeam.Trim(),
                AwayTeam = message.AwayTeam.Trim(),
                Fingerprint = primaryFingerprint,
                CreatedAtUtc = DateTime.UtcNow
            };

            dbContext.Games.Add(game);
            await dbContext.SaveChangesAsync(context.CancellationToken);

            await deduplicationService.MarkAsProcessedAsync(
                primaryFingerprint,
                DeduplicationTtl,
                context.CancellationToken);
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {
            logger.LogInformation("Duplicate game detected by unique constraint");
            await MarkFingerprintAsProcessedAsync(primaryFingerprint, context.CancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process game message");
            throw;
        }
    }

    private async Task MarkFingerprintAsProcessedAsync(string fingerprint, CancellationToken cancellationToken)
    {
        try
        {
            await deduplicationService.MarkAsProcessedAsync(fingerprint, DeduplicationTtl, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to mark duplicate fingerprint as processed");
        }
    }
}
