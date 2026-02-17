namespace SportsAggregator.Domain.Contracts;

public sealed class GetGamesRequest
{
    public string? Sport { get; init; }

    public DateTime? From { get; init; }

    public DateTime? To { get; init; }

    public string? Competition { get; init; }
}
