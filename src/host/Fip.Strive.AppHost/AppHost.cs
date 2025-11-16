using Fip.Strive.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

// External infrastructure
var user = builder.AddParameter("User");
var password = builder.AddParameter("Password", true);

var rabbit = builder.AddRabbitMQ("rabbitmq", user, password).WithManagementPlugin(8080);
var postgres = builder.AddPostgres("postgres").WithPgAdmin(pg => pg.WithHostPort(8081));
var redis = builder
    .AddRedis("redis")
    .WithDataVolume()
    .WithRedisInsight(ri => ri.WithHostPort(8082));

// Databases
var harvesterDatabase = postgres.AddDatabase("strive-harvester-database");

// Migrations
// Postgres Index Schema
var indexMigrator = builder.AddHarvestIndexMigrator(
    "strive-harvester-index-migrator",
    harvesterDatabase
);

// Postgres Queue Schema
var queueMigrator = builder.AddHarvestQueueMigrator(
    "strive-harvester-queue-migrator",
    harvesterDatabase
);

// Rabbit Pipeline Exchanges
var pipelineMigrator = builder.AddHarvestPipelineMigrator(
    "strive-harvester-pipeline-migrator",
    rabbit
);

// Pipeline
// Import unique zips
var pipelineZipGate = builder.AddHarvestPipelineZipGateStage(
    "strive-harvester-pipeline-zipgate",
    rabbit,
    redis,
    pipelineMigrator
);

// Unzip
var pipelineUnzipper = builder.AddHarvestPipelineUnzipperStage(
    "strive-harvester-pipeline-unzipper",
    rabbit,
    pipelineMigrator
);

// Scan unique files
var pipelineScanner = builder.AddHarvestPipelineScannerStage(
    "strive-harvester-pipeline-scanner",
    rabbit,
    redis,
    pipelineMigrator
);

// Synchronizer
// Sync redis indexes to postgres
var indexingSyncer = builder.AddHarvestIndexingSyncer(
    "strive-harvester-indexing-syncer",
    redis,
    harvesterDatabase
);

// Webapps
var harvesterWeb = builder
    .AddProject<Projects.Fip_Strive_Harvester_Web>("strive-harvester-webapp")
    .WithHttpHealthCheck("/health")
    .WithHttpHealthCheck("/health/queue")
    .WithReference(harvesterDatabase)
    .WithReference(rabbit)
    .WaitForCompletion(pipelineMigrator)
    .WaitForCompletion(indexMigrator)
    .WaitForCompletion(queueMigrator);

// var unifierWeb = builder
//     .AddProject<Projects.Fip_Strive_Unifier_Web>("strive-unifier-webapp")
//     .WithHttpHealthCheck("/health");

// var portalWeb = builder
//     .AddProject<Projects.Fip_Strive_Portal_Web>("strive-portal-webapp")
//     .WithHttpHealthCheck("/health")
//     .WithReference(unifierWeb)
//     .WithReference(harvesterWeb);

builder.Build().Run();
