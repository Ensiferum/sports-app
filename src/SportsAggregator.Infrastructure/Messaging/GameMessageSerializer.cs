using System.Text.Json;
using SportsAggregator.Domain.Contracts;

namespace SportsAggregator.Infrastructure.Messaging;

public static class GameMessageSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public static byte[] Serialize(IngestedGameMessage message)
    {
        return JsonSerializer.SerializeToUtf8Bytes(message, SerializerOptions);
    }

    public static IngestedGameMessage? Deserialize(ReadOnlyMemory<byte> payload)
    {
        return JsonSerializer.Deserialize<IngestedGameMessage>(payload.Span, SerializerOptions);
    }

    public static IngestedGameMessage? Deserialize(byte[] payload)
    {
        return JsonSerializer.Deserialize<IngestedGameMessage>(payload, SerializerOptions);
    }
}
