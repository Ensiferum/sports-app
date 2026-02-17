namespace SportsAggregator.Domain.Entities;

public sealed class Game
{
    public Guid Id { get; set; }

    public DateTime ScheduledAtUtc { get; set; }

    public string SportType { get; set; } = string.Empty;

    public string CompetitionName { get; set; } = string.Empty;

    public string HomeTeam { get; set; } = string.Empty;

    public string AwayTeam { get; set; } = string.Empty;

    public string MatchKey { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}
