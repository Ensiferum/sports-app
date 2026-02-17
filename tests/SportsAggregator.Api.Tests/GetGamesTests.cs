using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using SportsAggregator.Domain.Contracts;

namespace SportsAggregator.Api.Tests;

public sealed class GetGamesTests(ApiTestFixture fixture) : IClassFixture<ApiTestFixture>
{
    [Fact]
    public async Task GetGames_WithNoFilters_Returns200()
    {
        var response = await fixture.Client.GetAsync("/games");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetGames_WithSportFilter_ReturnsFilteredResults()
    {
        var allGames = await fixture.WaitForAnyGamesAsync();
        var sport = allGames[0].SportType;

        var filtered = await fixture.Client.GetFromJsonAsync<List<GameResponse>>(
            $"/games?sport={Uri.EscapeDataString(sport)}");

        filtered.Should().NotBeNull();
        filtered!.Should().OnlyContain(x => x.SportType == sport);
    }

    [Fact]
    public async Task GetGames_WithDateRange_ReturnsFilteredResults()
    {
        var allGames = await fixture.WaitForAnyGamesAsync();
        var pivot = allGames[0].ScheduledAtUtc;
        var from = pivot.AddHours(-1).ToString("O");
        var to = pivot.AddHours(1).ToString("O");

        var filtered = await fixture.Client.GetFromJsonAsync<List<GameResponse>>(
            $"/games?from={Uri.EscapeDataString(from)}&to={Uri.EscapeDataString(to)}");

        filtered.Should().NotBeNull();
        filtered!.Should().OnlyContain(x => x.ScheduledAtUtc >= pivot.AddHours(-1) && x.ScheduledAtUtc <= pivot.AddHours(1));
    }

    [Fact]
    public async Task GetGames_WithCompetition_ReturnsFilteredResults()
    {
        var allGames = await fixture.WaitForAnyGamesAsync();
        var competition = allGames[0].CompetitionName;
        var token = competition.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];

        var filtered = await fixture.Client.GetFromJsonAsync<List<GameResponse>>(
            $"/games?competition={Uri.EscapeDataString(token)}");

        filtered.Should().NotBeNull();
        filtered!.Should().OnlyContain(x =>
            x.CompetitionName.Contains(token, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetGames_FromAfterTo_Returns400()
    {
        var response = await fixture.Client.GetAsync("/games?from=2026-02-18T00:00:00Z&to=2026-02-17T00:00:00Z");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
