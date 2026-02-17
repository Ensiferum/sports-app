using MassTransit;
using SportsAggregator.Domain.Contracts;

namespace SportsAggregator.Ingestion.Services;

public sealed class GameMessagePublisher(IBus bus, ILogger<GameMessagePublisher> logger)
{
    public async Task PublishAsync(
        IReadOnlyList<IngestedGameMessage> messages,
        CancellationToken cancellationToken)
    {
        if (messages.Count == 0)
        {
            return;
        }

        foreach (var message in messages)
        {
            await bus.Publish(message, cancellationToken);
        }

        logger.LogInformation("Published {Count} game messages", messages.Count);
    }
}
