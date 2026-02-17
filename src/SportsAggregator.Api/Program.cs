using SportsAggregator.Api.Endpoints;
using SportsAggregator.Api.Services;
using SportsAggregator.Infrastructure.Data;
using SportsAggregator.Infrastructure.Services;
using Scalar.AspNetCore;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<SportsDbContext>("sportsdb");
builder.AddRedisClient("redis");
builder.AddRedisDistributedCache("redis");

builder.Services
    .AddFusionCache()
    .WithDefaultEntryOptions(new FusionCacheEntryOptions
    {
        Duration = TimeSpan.FromSeconds(30),
        IsFailSafeEnabled = true,
        FailSafeMaxDuration = TimeSpan.FromMinutes(5)
    })
    .WithSystemTextJsonSerializer()
    .WithRegisteredDistributedCache()
    .WithStackExchangeRedisBackplane(options =>
    {
        options.Configuration = builder.Configuration.GetConnectionString("redis");
    });

builder.Services.AddValidation();
builder.Services.AddOpenApi();
builder.Services.AddScoped<GameQueryService>();
builder.Services.AddScoped<IDeduplicationService, DeduplicationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapDefaultEndpoints();
app.MapGameEndpoints();

app.Run();
