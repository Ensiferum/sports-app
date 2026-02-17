using Microsoft.EntityFrameworkCore;
using SportsAggregator.Domain.Contracts;
using SportsAggregator.Domain.Results;
using SportsAggregator.Infrastructure.Data;
using ZiggyCreatures.Caching.Fusion;

namespace SportsAggregator.Api.Services;

public sealed class GameQueryService(
    SportsDbContext dbContext,
    IFusionCache cache,
    ILogger<GameQueryService> logger)
{
    public async Task<Result<IReadOnlyList<GameResponse>>> GetGamesAsync(
        GetGamesRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var cacheKey = BuildCacheKey(request);

            var games = await cache.GetOrSetAsync(
                cacheKey,
                async token => await QueryAsync(request, token),
                options =>
                {
                    options.Duration = TimeSpan.FromSeconds(10);
                    options.IsFailSafeEnabled = true;
                    options.FailSafeMaxDuration = TimeSpan.FromMinutes(5);
                },
                token: cancellationToken);

            return Result<IReadOnlyList<GameResponse>>.Success(games);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to query games");
            return Result<IReadOnlyList<GameResponse>>.Failure(
                new Error("games.query_failed", "Failed to retrieve games.", ErrorType.Unexpected));
        }
    }

    private async Task<IReadOnlyList<GameResponse>> QueryAsync(
        GetGamesRequest request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Games.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Sport))
        {
            var sport = request.Sport.Trim().ToLowerInvariant();
            query = query.Where(g => g.SportType == sport);
        }

        if (!string.IsNullOrWhiteSpace(request.Competition))
        {
            var competitionPattern = $"%{request.Competition.Trim()}%";
            query = query.Where(g => EF.Functions.ILike(g.CompetitionName, competitionPattern));
        }

        if (request.From.HasValue)
        {
            var fromUtc = EnsureUtc(request.From.Value);
            query = query.Where(g => g.ScheduledAtUtc >= fromUtc);
        }

        if (request.To.HasValue)
        {
            var toUtc = EnsureUtc(request.To.Value);
            query = query.Where(g => g.ScheduledAtUtc <= toUtc);
        }

        var items = await query
            .OrderByDescending(g => g.ScheduledAtUtc)
            .Select(g => new GameResponse(
                g.Id,
                g.ScheduledAtUtc,
                g.SportType,
                g.CompetitionName,
                g.HomeTeam,
                g.AwayTeam,
                g.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return items;
    }

    private static string BuildCacheKey(GetGamesRequest request)
    {
        return string.Join(
            ':',
            "games",
            request.Sport?.Trim().ToLowerInvariant() ?? string.Empty,
            request.From?.ToUniversalTime().ToString("O") ?? string.Empty,
            request.To?.ToUniversalTime().ToString("O") ?? string.Empty,
            request.Competition?.Trim().ToLowerInvariant() ?? string.Empty);
    }

    private static DateTime EnsureUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
    }
}
