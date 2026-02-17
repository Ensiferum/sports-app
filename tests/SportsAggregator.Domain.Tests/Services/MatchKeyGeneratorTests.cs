using AwesomeAssertions;
using SportsAggregator.Domain.Services;

namespace SportsAggregator.Domain.Tests.Services;

public class MatchKeyGeneratorTests
{
    [Fact]
    public void Generate_WithSameInputs_IsDeterministic()
    {
        var first = MatchKeyGenerator.Generate("football", "premier league", "arsenal", "chelsea");
        var second = MatchKeyGenerator.Generate("football", "premier league", "arsenal", "chelsea");

        first.Should().Be(second);
    }

    [Fact]
    public void Generate_WhenTeamsAreSwapped_IsOrderInsensitive()
    {
        var first = MatchKeyGenerator.Generate("football", "premier league", "arsenal", "chelsea");
        var second = MatchKeyGenerator.Generate("football", "premier league", "chelsea", "arsenal");

        first.Should().Be(second);
    }

    [Fact]
    public void Generate_WithDifferentCase_ProducesSameMatchKey()
    {
        var first = MatchKeyGenerator.Generate("Football", "Premier League", "Arsenal", "Chelsea");
        var second = MatchKeyGenerator.Generate("football", "premier league", "arsenal", "chelsea");

        first.Should().Be(second);
    }

    [Fact]
    public void Generate_WithExtraWhitespace_ProducesSameMatchKey()
    {
        var first = MatchKeyGenerator.Generate(" football ", " premier league ", " arsenal ", " chelsea ");
        var second = MatchKeyGenerator.Generate("football", "premier league", "arsenal", "chelsea");

        first.Should().Be(second);
    }

    [Fact]
    public void Generate_WithDifferentCoreValues_ProducesDifferentMatchKeys()
    {
        var baseline = MatchKeyGenerator.Generate("football", "premier league", "arsenal", "chelsea");

        var differentSport = MatchKeyGenerator.Generate("basketball", "premier league", "arsenal", "chelsea");
        var differentCompetition = MatchKeyGenerator.Generate("football", "la liga", "arsenal", "chelsea");
        var differentTeams = MatchKeyGenerator.Generate("football", "premier league", "arsenal", "liverpool");

        differentSport.Should().NotBe(baseline);
        differentCompetition.Should().NotBe(baseline);
        differentTeams.Should().NotBe(baseline);
    }

    [Fact]
    public void Normalize_TrimsAndLowercasesInput()
    {
        MatchKeyGenerator.Normalize("  Premier League  ").Should().Be("premier league");
    }
}
