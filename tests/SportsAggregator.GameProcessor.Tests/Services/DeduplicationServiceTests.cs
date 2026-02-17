using AwesomeAssertions;
using NSubstitute.Core;
using NSubstitute;
using SportsAggregator.Infrastructure.Services;
using StackExchange.Redis;

namespace SportsAggregator.GameProcessor.Tests.Services;

public class DeduplicationServiceTests
{
    [Fact]
    public async Task IsDuplicateAsync_WhenPrimaryExists_ReturnsTrue()
    {
        var database = Substitute.For<IDatabase>();
        database.KeyExistsAsync("primary", Arg.Any<CommandFlags>()).Returns(true);
        database.KeyExistsAsync("adjacent", Arg.Any<CommandFlags>()).Returns(false);

        var multiplexer = Substitute.For<IConnectionMultiplexer>();
        multiplexer.GetDatabase().Returns(database);

        var service = new DeduplicationService(multiplexer);

        var isDuplicate = await service.IsDuplicateAsync("primary", "adjacent");

        isDuplicate.Should().BeTrue();
    }

    [Fact]
    public async Task IsDuplicateAsync_WhenNeitherExists_ReturnsFalse()
    {
        var database = Substitute.For<IDatabase>();
        database.KeyExistsAsync("primary", Arg.Any<CommandFlags>()).Returns(false);
        database.KeyExistsAsync("adjacent", Arg.Any<CommandFlags>()).Returns(false);

        var multiplexer = Substitute.For<IConnectionMultiplexer>();
        multiplexer.GetDatabase().Returns(database);

        var service = new DeduplicationService(multiplexer);

        var isDuplicate = await service.IsDuplicateAsync("primary", "adjacent");

        isDuplicate.Should().BeFalse();
    }

    [Fact]
    public async Task MarkAsProcessedAsync_SetsRedisKeyWithExpiry()
    {
        var database = Substitute.For<IDatabase>();

        var multiplexer = Substitute.For<IConnectionMultiplexer>();
        multiplexer.GetDatabase().Returns(database);

        var service = new DeduplicationService(multiplexer);
        var expiry = TimeSpan.FromHours(2);

        await service.MarkAsProcessedAsync("fp", expiry);

        var writeCall = database.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == "StringSetAsync")
            .Single();

        var args = writeCall.GetArguments();
        ((RedisKey)args[0]!).ToString().Should().Be("fp");
        args.Length.Should().BeGreaterThan(2);
        args[2].Should().NotBeNull();
    }
}
