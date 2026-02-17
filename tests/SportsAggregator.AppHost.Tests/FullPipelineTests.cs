using System.Net.Http.Json;
using Aspire.Hosting.Testing;
using AwesomeAssertions;
using Projects;
using SportsAggregator.Domain.Contracts;

namespace SportsAggregator.AppHost.Tests;

public sealed class FullPipelineTests
{
    private static readonly TimeSpan StartupTimeout = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan IngestionTimeout = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);

    [Fact(Timeout = 300_000)]
    public async Task FullPipeline_IngestsGames_AndAvoidsDuplicates()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<SportsAggregator_AppHost>();
        await using var app = await appHost.BuildAsync();

        using var startupCts = new CancellationTokenSource(StartupTimeout);
        await app.StartAsync(startupCts.Token);
        await app.ResourceNotifications.WaitForResourceHealthyAsync("api", startupCts.Token);
        await app.ResourceNotifications.WaitForResourceHealthyAsync("ingestion", startupCts.Token);
        await app.ResourceNotifications.WaitForResourceHealthyAsync("game-processor", startupCts.Token);

        using var client = app.CreateHttpClient("api", "http");
        client.Timeout = TimeSpan.FromSeconds(15);

        var response = await WaitForGamesAsync(client, IngestionTimeout, CancellationToken.None);

        response.Count.Should().BeGreaterThan(0);

        var grouped = response
            .GroupBy(g => new
            {
                g.SportType,
                g.CompetitionName,
                TeamA = string.Compare(g.HomeTeam, g.AwayTeam, StringComparison.OrdinalIgnoreCase) <= 0
                    ? g.HomeTeam.Trim().ToLowerInvariant()
                    : g.AwayTeam.Trim().ToLowerInvariant(),
                TeamB = string.Compare(g.HomeTeam, g.AwayTeam, StringComparison.OrdinalIgnoreCase) <= 0
                    ? g.AwayTeam.Trim().ToLowerInvariant()
                    : g.HomeTeam.Trim().ToLowerInvariant(),
                Bucket = new DateTime(
                    g.ScheduledAtUtc.Year,
                    g.ScheduledAtUtc.Month,
                    g.ScheduledAtUtc.Day,
                    g.ScheduledAtUtc.Hour / 2 * 2,
                    0,
                    0,
                    DateTimeKind.Utc)
            })
            .ToList();

        grouped.Should().OnlyContain(group => group.Count() == 1);
    }

    private static async Task<IReadOnlyList<GameResponse>> WaitForGamesAsync(
        HttpClient client,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var deadline = DateTime.UtcNow + timeout;
        string lastObservation = "No successful response received.";

        for (var attempt = 1; DateTime.UtcNow < deadline; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using var requestCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                requestCts.CancelAfter(TimeSpan.FromSeconds(10));

                using var httpResponse = await client.GetAsync(
                    "/games",
                    requestCts.Token);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    lastObservation =
                        $"Attempt {attempt}: HTTP {(int)httpResponse.StatusCode} ({httpResponse.StatusCode}).";
                }
                else
                {
                    var response = await httpResponse.Content.ReadFromJsonAsync<List<GameResponse>>(
                        cancellationToken: requestCts.Token);

                    if (response is not null && response.Count > 0)
                    {
                        return response;
                    }

                    lastObservation = $"Attempt {attempt}: HTTP 200 with no ingested games yet.";
                }
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                lastObservation = $"Attempt {attempt}: request timed out.";
            }
            catch (HttpRequestException ex)
            {
                lastObservation = $"Attempt {attempt}: request failed with '{ex.Message}'.";
            }

            var remaining = deadline - DateTime.UtcNow;
            if (remaining > TimeSpan.Zero)
            {
                await Task.Delay(remaining < PollInterval ? remaining : PollInterval, cancellationToken);
            }
        }

        throw new TimeoutException(
            $"No games were ingested within {timeout}. Last observed state: {lastObservation}");
    }
}
