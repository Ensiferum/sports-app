using SportsAggregator.Domain.Contracts;

namespace SportsAggregator.Ingestion.Abstractions;

public interface IGameSource
{
    string SourceName { get; }

    Task<IReadOnlyList<IngestedGameMessage>> FetchGamesAsync(CancellationToken cancellationToken);
}
