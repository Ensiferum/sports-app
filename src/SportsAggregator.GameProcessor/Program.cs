using MassTransit;
using SportsAggregator.GameProcessor;
using SportsAggregator.Domain.Contracts;
using SportsAggregator.Infrastructure.Data;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<SportsDbContext>("sportsdb");

builder.Services.AddMassTransit(bus =>
{
    bus.AddConsumer<GameMessageConsumer>();

    bus.UsingRabbitMq((context, cfg) =>
    {
        var connectionString = builder.Configuration.GetConnectionString("rabbitmq")
            ?? throw new InvalidOperationException("Connection string 'rabbitmq' is missing.");

        cfg.Host(new Uri(connectionString));
        cfg.ReceiveEndpoint(QueueConstants.QueueName, endpoint =>
        {
            endpoint.PrefetchCount = 1;
            endpoint.ConcurrentMessageLimit = 1;
            endpoint.ConfigureConsumer<GameMessageConsumer>(context);
        });
    });
});

var host = builder.Build();
host.Run();
