using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace SportsAggregator.Domain.Services;

public static class FingerprintGenerator
{
    private static readonly TimeSpan BucketSize = TimeSpan.FromHours(2);

    public static string Generate(
        string sport,
        string competition,
        string team1,
        string team2,
        DateTime scheduledAtUtc)
    {
        var normalizedSport = Normalize(sport);
        var normalizedCompetition = Normalize(competition);
        var normalizedTeam1 = Normalize(team1);
        var normalizedTeam2 = Normalize(team2);

        var orderedTeams = new[] { normalizedTeam1, normalizedTeam2 }
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        var bucketStart = GetBucketStart(scheduledAtUtc);
        var bucketToken = bucketStart.ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture);

        var compositeKey = string.Join(
            "|",
            normalizedSport,
            normalizedCompetition,
            orderedTeams[0],
            orderedTeams[1],
            bucketToken);

        return ComputeSha256(compositeKey);
    }

    public static string GenerateAdjacentBucket(
        string sport,
        string competition,
        string team1,
        string team2,
        DateTime scheduledAtUtc)
    {
        var bucketStart = GetBucketStart(scheduledAtUtc);
        var boundary = bucketStart + BucketSize;

        var distanceToPrevious = scheduledAtUtc - bucketStart;
        var distanceToNext = boundary - scheduledAtUtc;

        var adjacentTimestamp = distanceToPrevious <= distanceToNext
            ? scheduledAtUtc - BucketSize
            : scheduledAtUtc + BucketSize;

        return Generate(sport, competition, team1, team2, adjacentTimestamp);
    }

    private static string Normalize(string value)
    {
        return value.Trim().ToLowerInvariant();
    }

    private static DateTime GetBucketStart(DateTime utcDateTime)
    {
        var normalized = utcDateTime.Kind == DateTimeKind.Utc
            ? utcDateTime
            : utcDateTime.ToUniversalTime();

        var startOfDay = new DateTime(
            normalized.Year,
            normalized.Month,
            normalized.Day,
            0,
            0,
            0,
            DateTimeKind.Utc);

        var elapsed = normalized - startOfDay;
        var bucketIndex = (int)Math.Floor(elapsed.TotalHours / BucketSize.TotalHours);

        return startOfDay.AddHours(bucketIndex * BucketSize.TotalHours);
    }

    private static string ComputeSha256(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
