using Microsoft.Extensions.Options;
using SportsAggregator.Ingestion.Abstractions;
using SportsAggregator.Ingestion.Services;

namespace SportsAggregator.Ingestion;

public sealed class SourceIngestionWorker<TSource>(
    TSource source,
    GameMessagePublisher messagePublisher,
    IOptions<IngestionOptions> options,
    ILogger<SourceIngestionWorker<TSource>> logger) : BackgroundService
    where TSource : class, IGameSource
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var maxIntervalSeconds = Math.Max(1, options.Value.IngestionIntervalSeconds);
        logger.LogInformation("Starting ingestion worker for source {SourceName}", source.SourceName);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var games = await source.FetchGamesAsync(stoppingToken);
                await messagePublisher.PublishAsync(games, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Source {SourceName} failed", source.SourceName);
            }

            var delay = GetRandomDelay(maxIntervalSeconds);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        logger.LogInformation("Stopped ingestion worker for source {SourceName}", source.SourceName);
    }

    private static TimeSpan GetRandomDelay(int maxIntervalSeconds)
    {
        var delaySeconds = Random.Shared.NextInt64(1, (long)maxIntervalSeconds + 1);
        return TimeSpan.FromSeconds(delaySeconds);
    }
}
