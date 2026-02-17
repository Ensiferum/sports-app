namespace SportsAggregator.Domain.Contracts;

public sealed record IngestedGameMessage(
    string SportType,
    string CompetitionName,
    string HomeTeam,
    string AwayTeam,
    DateTime ScheduledAtUtc,
    string Source,
    DateTime IngestedAtUtc);
