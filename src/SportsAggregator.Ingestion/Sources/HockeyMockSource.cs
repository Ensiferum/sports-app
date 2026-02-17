using SportsAggregator.Domain;
using SportsAggregator.Domain.Contracts;
using SportsAggregator.Ingestion.Abstractions;

namespace SportsAggregator.Ingestion.Sources;

public sealed class HockeyMockSource : IGameSource
{
    private static readonly string[] Competitions =
    [
        "NHL",
        "KHL",
        "IIHF World Championship",
        "Champions Hockey League"
    ];

    private static readonly string[] Teams =
    [
        "Rangers",
        "Bruins",
        "Maple Leafs",
        "Canadiens",
        "Red Wings",
        "Penguins",
        "Oilers",
        "Flames",
        "Avalanche",
        "Golden Knights",
        "CSKA Moscow",
        "SKA Saint Petersburg"
    ];

    public string SourceName => "hockey-mock";

    public Task<IReadOnlyList<IngestedGameMessage>> FetchGamesAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var count = Random.Shared.Next(1, 5);
        var results = new List<IngestedGameMessage>(count);

        for (var i = 0; i < count; i++)
        {
            var teams = PickDistinct(Teams);
            results.Add(new IngestedGameMessage(
                SportTypes.IceHockey,
                Competitions[Random.Shared.Next(Competitions.Length)],
                teams.home,
                teams.away,
                now.AddMinutes(Random.Shared.Next(20, 60 * 16)),
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
