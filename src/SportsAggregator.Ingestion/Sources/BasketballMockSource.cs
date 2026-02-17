using SportsAggregator.Domain;
using SportsAggregator.Domain.Contracts;
using SportsAggregator.Ingestion.Abstractions;

namespace SportsAggregator.Ingestion.Sources;

public sealed class BasketballMockSource : IGameSource
{
    private static readonly string[] Competitions =
    [
        "NBA",
        "EuroLeague"
    ];

    private static readonly string[] Teams =
    [
        "Lakers",
        "Warriors",
        "Celtics",
        "Bulls",
        "Nuggets",
        "Heat",
        "Bucks",
        "Suns",
        "Mavericks",
        "Clippers",
        "Real Madrid Basket",
        "Barcelona Basket",
        "Fenerbahce",
        "Olympiacos"
    ];

    public string SourceName => "basketball-mock";

    public Task<IReadOnlyList<IngestedGameMessage>> FetchGamesAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var count = Random.Shared.Next(1, 5);
        var results = new List<IngestedGameMessage>(count);

        for (var i = 0; i < count; i++)
        {
            var teams = PickDistinct(Teams);
            results.Add(new IngestedGameMessage(
                SportTypes.Basketball,
                Competitions[Random.Shared.Next(Competitions.Length)],
                teams.home,
                teams.away,
                now.AddMinutes(Random.Shared.Next(30, 60 * 24)),
                SourceName,
                now));
        }

        return Task.FromResult<IReadOnlyList<IngestedGameMessage>>(results);
    }

    private static (string home, string away) PickDistinct(IReadOnlyList<string> candidates)
    {
        var first = Random.Shared.Next(candidates.Count);
        var second = Random.Shared.Next(candidates.Count);

        while (second == first)
        {
            second = Random.Shared.Next(candidates.Count);
        }

        return (candidates[first], candidates[second]);
    }
}
