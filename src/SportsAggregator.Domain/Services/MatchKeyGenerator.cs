using System.Security.Cryptography;
using System.Text;

namespace SportsAggregator.Domain.Services;

public static class MatchKeyGenerator
{
    public static string Generate(string sport, string competition, string team1, string team2)
    {
        var normalizedSport = Normalize(sport);
        var normalizedCompetition = Normalize(competition);
        var normalizedTeam1 = Normalize(team1);
        var normalizedTeam2 = Normalize(team2);

        var orderedTeams = new[] { normalizedTeam1, normalizedTeam2 }
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        var compositeKey = string.Join(
            "|",
            normalizedSport,
            normalizedCompetition,
            orderedTeams[0],
            orderedTeams[1]);

        return ComputeSha256(compositeKey);
    }

    public static string Normalize(string value)
    {
        return value.Trim().ToLowerInvariant();
    }

    private static string ComputeSha256(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
