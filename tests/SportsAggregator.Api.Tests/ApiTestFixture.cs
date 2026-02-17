using System.Net.Http.Json;
using Aspire.Hosting.Testing;
using Projects;
using SportsAggregator.Domain.Contracts;

namespace SportsAggregator.Api.Tests;

public sealed class ApiTestFixture : IAsyncLifetime
{
    private static readonly TimeSpan StartupTimeout = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan IngestionTimeout = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);

    private Aspire.Hosting.DistributedApplication? _app;

    public HttpClient Client { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<SportsAggregator_AppHost>();
        _app = await appHost.BuildAsync();

        using var startupCts = new CancellationTokenSource(StartupTimeout);
        await _app.StartAsync(startupCts.Token);
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("api", startupCts.Token);
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("ingestion", startupCts.Token);
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("game-processor", startupCts.Token);

        Client = _app.CreateHttpClient("api", "http");
        Client.Timeout = TimeSpan.FromSeconds(15);

        await WaitForAnyGamesAsync();
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();

        if (_app is not null)
        {
            await _app.DisposeAsync();
        }
    }

    public async Task<IReadOnlyList<GameResponse>> WaitForAnyGamesAsync(CancellationToken cancellationToken = default)
    {
        var deadline = DateTime.UtcNow + IngestionTimeout;
        string lastObservation = "No successful response received.";

        for (var attempt = 1; DateTime.UtcNow < deadline; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using var requestCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                requestCts.CancelAfter(TimeSpan.FromSeconds(10));

                using var httpResponse = await Client.GetAsync(
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
            $"No games were ingested within {IngestionTimeout}. Last observed state: {lastObservation}");
    }
}
