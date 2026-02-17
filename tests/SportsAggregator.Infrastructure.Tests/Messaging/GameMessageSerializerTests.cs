using AwesomeAssertions;
using SportsAggregator.Domain.Contracts;
using SportsAggregator.Infrastructure.Messaging;

namespace SportsAggregator.Infrastructure.Tests.Messaging;

public class GameMessageSerializerTests
{
    [Fact]
    public void SerializeDeserialize_RoundTripsMessage()
    {
        var message = new IngestedGameMessage(
            "football",
            "Premier League",
            "Arsenal",
            "Chelsea",
            new DateTime(2026, 2, 16, 19, 0, 0, DateTimeKind.Utc),
            "football-mock",
            new DateTime(2026, 2, 16, 18, 30, 0, DateTimeKind.Utc));

        var payload = GameMessageSerializer.Serialize(message);
        var deserialized = GameMessageSerializer.Deserialize(payload);

        deserialized.Should().NotBeNull();
        deserialized.Should().BeEquivalentTo(message);
    }

    [Fact]
    public void Deserialize_InvalidPayload_ReturnsNull()
    {
        var payload = new byte[] { 0x42, 0x43, 0x44 };

        var act = () => GameMessageSerializer.Deserialize(payload);

        act.Should().Throw<Exception>();
    }
}
