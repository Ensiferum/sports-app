using AwesomeAssertions;
using SportsAggregator.Domain.Services;

namespace SportsAggregator.Domain.Tests.Services;

public class FingerprintGeneratorTests
{
    [Fact]
    public void Generate_WithSameInputs_IsDeterministic()
    {
        var scheduledAt = new DateTime(2026, 2, 16, 10, 15, 0, DateTimeKind.Utc);

        var first = FingerprintGenerator.Generate("football", "premier league", "arsenal", "chelsea", scheduledAt);
        var second = FingerprintGenerator.Generate("football", "premier league", "arsenal", "chelsea", scheduledAt);

        first.Should().Be(second);
    }

    [Fact]
    public void Generate_WhenTeamsAreSwapped_IsOrderInsensitive()
    {
        var scheduledAt = new DateTime(2026, 2, 16, 10, 15, 0, DateTimeKind.Utc);

        var first = FingerprintGenerator.Generate("football", "premier league", "arsenal", "chelsea", scheduledAt);
        var second = FingerprintGenerator.Generate("football", "premier league", "chelsea", "arsenal", scheduledAt);

        first.Should().Be(second);
    }

    [Fact]
    public void Generate_WithDifferentCase_ProducesSameFingerprint()
    {
        var scheduledAt = new DateTime(2026, 2, 16, 10, 15, 0, DateTimeKind.Utc);

        var first = FingerprintGenerator.Generate("Football", "Premier League", "Arsenal", "Chelsea", scheduledAt);
        var second = FingerprintGenerator.Generate("football", "premier league", "arsenal", "chelsea", scheduledAt);

        first.Should().Be(second);
    }

    [Fact]
    public void Generate_WithExtraWhitespace_ProducesSameFingerprint()
    {
        var scheduledAt = new DateTime(2026, 2, 16, 10, 15, 0, DateTimeKind.Utc);

        var first = FingerprintGenerator.Generate(" football ", " premier league ", " arsenal ", " chelsea ", scheduledAt);
        var second = FingerprintGenerator.Generate("football", "premier league", "arsenal", "chelsea", scheduledAt);

        first.Should().Be(second);
    }

    [Fact]
    public void Generate_WhenInSameTwoHourBucket_ProducesSameFingerprint()
    {
        var first = FingerprintGenerator.Generate(
            "football",
            "premier league",
            "arsenal",
            "chelsea",
            new DateTime(2026, 2, 16, 10, 10, 0, DateTimeKind.Utc));

        var second = FingerprintGenerator.Generate(
            "football",
            "premier league",
            "arsenal",
            "chelsea",
            new DateTime(2026, 2, 16, 11, 50, 0, DateTimeKind.Utc));

        first.Should().Be(second);
    }

    [Fact]
    public void Generate_WhenInDifferentBuckets_ProducesDifferentFingerprint()
    {
        var first = FingerprintGenerator.Generate(
            "football",
            "premier league",
            "arsenal",
            "chelsea",
            new DateTime(2026, 2, 16, 1, 50, 0, DateTimeKind.Utc));

        var second = FingerprintGenerator.Generate(
            "football",
            "premier league",
            "arsenal",
            "chelsea",
            new DateTime(2026, 2, 16, 2, 10, 0, DateTimeKind.Utc));

        first.Should().NotBe(second);
    }

    [Fact]
    public void Generate_WithDifferentCoreValues_ProducesDifferentFingerprint()
    {
        var scheduledAt = new DateTime(2026, 2, 16, 10, 15, 0, DateTimeKind.Utc);

        var baseline = FingerprintGenerator.Generate("football", "premier league", "arsenal", "chelsea", scheduledAt);

        var differentSport = FingerprintGenerator.Generate("basketball", "premier league", "arsenal", "chelsea", scheduledAt);
        var differentCompetition = FingerprintGenerator.Generate("football", "la liga", "arsenal", "chelsea", scheduledAt);
        var differentTeams = FingerprintGenerator.Generate("football", "premier league", "arsenal", "liverpool", scheduledAt);

        differentSport.Should().NotBe(baseline);
        differentCompetition.Should().NotBe(baseline);
        differentTeams.Should().NotBe(baseline);
    }

    [Fact]
    public void GenerateAdjacentBucket_WhenNearBoundary_UsesNeighboringBucket()
    {
        var nearBoundary = new DateTime(2026, 2, 16, 1, 50, 0, DateTimeKind.Utc);

        var adjacent = FingerprintGenerator.GenerateAdjacentBucket(
            "football",
            "premier league",
            "arsenal",
            "chelsea",
            nearBoundary);

        var expectedNeighbor = FingerprintGenerator.Generate(
            "football",
            "premier league",
            "arsenal",
            "chelsea",
            nearBoundary.AddMinutes(20));

        adjacent.Should().Be(expectedNeighbor);
    }
}
