using MassTransit;
using SportsAggregator.Ingestion;
using SportsAggregator.Ingestion.Abstractions;
using SportsAggregator.Ingestion.Services;
using SportsAggregator.Ingestion.Sources;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddMassTransit(bus =>
{
    bus.UsingRabbitMq((_, cfg) =>
    {
        var connectionString = builder.Configuration.GetConnectionString("rabbitmq")
            ?? throw new InvalidOperationException("Connection string 'rabbitmq' is missing.");

        cfg.Host(new Uri(connectionString));
    });
});

builder.Services.Configure<IngestionOptions>(builder.Configuration.GetSection(IngestionOptions.SectionName));

builder.Services.AddSingleton<FootballMockSource>();
builder.Services.AddSingleton<BasketballMockSource>();
builder.Services.AddSingleton<HockeyMockSource>();

builder.Services.AddSingleton<IGameSource>(sp => sp.GetRequiredService<FootballMockSource>());
builder.Services.AddSingleton<IGameSource>(sp => sp.GetRequiredService<BasketballMockSource>());
builder.Services.AddSingleton<IGameSource>(sp => sp.GetRequiredService<HockeyMockSource>());

builder.Services.AddSingleton<GameMessagePublisher>();
builder.Services.AddHostedService<SourceIngestionWorker<FootballMockSource>>();
builder.Services.AddHostedService<SourceIngestionWorker<BasketballMockSource>>();
builder.Services.AddHostedService<SourceIngestionWorker<HockeyMockSource>>();

var host = builder.Build();
host.Run();
