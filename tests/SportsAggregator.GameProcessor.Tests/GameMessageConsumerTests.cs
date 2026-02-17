using AwesomeAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SportsAggregator.Domain.Contracts;
using SportsAggregator.GameProcessor;
using SportsAggregator.Infrastructure.Data;

namespace SportsAggregator.GameProcessor.Tests;

public class GameMessageConsumerTests
{
    [Fact]
    public async Task Consume_WhenSameMatchWithinTwoHours_InsertsOnlyOneGame()
    {
        await using var dbContext = CreateDbContext();
        var consumer = new GameMessageConsumer(dbContext, NullLogger<GameMessageConsumer>.Instance);

        var firstMessage = CreateMessage(new DateTime(2026, 2, 17, 10, 0, 0, DateTimeKind.Utc));
        var secondMessage = CreateMessage(new DateTime(2026, 2, 17, 11, 45, 0, DateTimeKind.Utc));

        await consumer.Consume(CreateConsumeContext(firstMessage));
        await consumer.Consume(CreateConsumeContext(secondMessage));

        var games = await dbContext.Games.AsNoTracking().ToListAsync();

        games.Should().HaveCount(1);
    }

    [Fact]
    public async Task Consume_WhenSameMatchMoreThanTwoHoursApart_InsertsBothGames()
    {
        await using var dbContext = CreateDbContext();
        var consumer = new GameMessageConsumer(dbContext, NullLogger<GameMessageConsumer>.Instance);

        var firstMessage = CreateMessage(new DateTime(2026, 2, 17, 10, 0, 0, DateTimeKind.Utc));
        var secondMessage = CreateMessage(new DateTime(2026, 2, 17, 12, 1, 0, DateTimeKind.Utc));

        await consumer.Consume(CreateConsumeContext(firstMessage));
        await consumer.Consume(CreateConsumeContext(secondMessage));

        var games = await dbContext.Games.AsNoTracking().OrderBy(game => game.ScheduledAtUtc).ToListAsync();

        games.Should().HaveCount(2);
    }

    private static SportsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<SportsDbContext>()
            .UseInMemoryDatabase($"sports-db-{Guid.NewGuid()}")
            .Options;

        return new SportsDbContext(options);
    }

    private static IngestedGameMessage CreateMessage(DateTime scheduledAtUtc)
    {
        return new IngestedGameMessage(
            "football",
            "Premier League",
            "Arsenal",
            "Chelsea",
            scheduledAtUtc,
            "test-source",
            DateTime.UtcNow);
    }

    private static ConsumeContext<IngestedGameMessage> CreateConsumeContext(IngestedGameMessage message)
    {
        var context = Substitute.For<ConsumeContext<IngestedGameMessage>>();
        context.Message.Returns(message);
        context.CancellationToken.Returns(CancellationToken.None);
        return context;
    }
}
