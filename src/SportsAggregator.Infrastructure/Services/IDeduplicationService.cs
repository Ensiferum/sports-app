namespace SportsAggregator.Infrastructure.Services;

public interface IDeduplicationService
{
    Task<bool> IsDuplicateAsync(
        string primaryFingerprint,
        string adjacentFingerprint,
        CancellationToken cancellationToken = default);

    Task MarkAsProcessedAsync(
        string fingerprint,
        TimeSpan expiry,
        CancellationToken cancellationToken = default);
}
