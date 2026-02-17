namespace SportsAggregator.Domain.Contracts;

public sealed record GameResponse(
    Guid Id,
    DateTime ScheduledAtUtc,
    string SportType,
    string CompetitionName,
    string HomeTeam,
    string AwayTeam,
    DateTime CreatedAtUtc);
