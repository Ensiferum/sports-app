using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder
    .AddPostgres("postgres")
    .WithPgAdmin();

var sportsDatabase = postgres.AddDatabase("sportsdb");

var redis = builder
    .AddRedis("redis");

var rabbitMq = builder
    .AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

var dbMigration = builder
    .AddProject<Projects.SportsAggregator_DbMigrationService>("db-migration")
    .WithReference(sportsDatabase)
    .WaitFor(sportsDatabase);

var gameProcessor = builder
    .AddProject<Projects.SportsAggregator_GameProcessor>("game-processor")
    .WithReference(sportsDatabase)
    .WithReference(redis)
    .WithReference(rabbitMq)
    .WaitFor(sportsDatabase)
    .WaitFor(redis)
    .WaitFor(rabbitMq)
    .WaitForCompletion(dbMigration);

var ingestion = builder
    .AddProject<Projects.SportsAggregator_Ingestion>("ingestion")
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq);

var api = builder
    .AddProject<Projects.SportsAggregator_Api>("api")
    .WithReference(sportsDatabase)
    .WithReference(redis)
    .WaitFor(sportsDatabase)
    .WaitFor(redis)
    .WaitForCompletion(dbMigration)
    .WithExternalHttpEndpoints()
    .WithUrlForEndpoint("http", ep => new()
    {
        Url = "/scalar",
        DisplayText = "Scalar"
    });

builder.Build().Run();
