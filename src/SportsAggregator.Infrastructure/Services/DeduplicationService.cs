using StackExchange.Redis;

namespace SportsAggregator.Infrastructure.Services;

public sealed class DeduplicationService(IConnectionMultiplexer connectionMultiplexer) : IDeduplicationService
{
    private readonly IDatabase _database = connectionMultiplexer.GetDatabase();

    public async Task<bool> IsDuplicateAsync(
        string primaryFingerprint,
        string adjacentFingerprint,
        CancellationToken cancellationToken = default)
    {
        var primaryExistsTask = _database.KeyExistsAsync(primaryFingerprint);
        var adjacentExistsTask = _database.KeyExistsAsync(adjacentFingerprint);

        await Task.WhenAll(primaryExistsTask, adjacentExistsTask);

        return primaryExistsTask.Result || adjacentExistsTask.Result;
    }

    public Task MarkAsProcessedAsync(
        string fingerprint,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        return _database.StringSetAsync(fingerprint, "1", expiry);
    }
}
