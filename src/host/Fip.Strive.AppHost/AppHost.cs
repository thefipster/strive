using Fip.Strive.AppHost.Extensions;

const string zipsPath = @"E:\strive\data\imports\zips";
const string filesPath = @"E:\strive\data\imports\files";

var builder = DistributedApplication.CreateBuilder(args);

// External infrastructure
var user = builder.AddParameter("User");
var password = builder.AddParameter("Password", true);

var rabbit = builder.AddRabbitMQ("rabbitmq", user, password).WithManagementPlugin(8080);
var postgres = builder.AddPostgres("postgres").WithPgAdmin(pg => pg.WithHostPort(8081));
var redis = builder.AddRedis("redis").WithRedisInsight(ri => ri.WithHostPort(8082));

// Databases
var harvesterDatabase = postgres.AddDatabase("strive-harvester-database");

// Migrations
// Postgres Index Schema
var indexMigrator = builder.AddDependentResource<Projects.Fip_Strive_Harvester_Indexing_Migrator>(
    "strive-harvester-index-migrator",
    [harvesterDatabase]
);

// Postgres Queue Schema
var queueMigrator =
    builder.AddDependentResource<Projects.Fip_Strive_Queue_Storage_Postgres_Migrator>(
        "strive-harvester-queue-migrator",
        [harvesterDatabase]
    );

// Rabbit Pipeline Exchanges
var pipelineMigrator =
    builder.AddDependentResource<Projects.Fip_Strive_Harvester_Pipeline_Migrator>(
        "strive-harvester-pipeline-migrator",
        [rabbit]
    );

// Pipeline
// Import unique zips
var pipelineZipGate =
    builder.AddDependentResourceWithSequence<Projects.Fip_Strive_Harvester_Pipeline_ZipGate_Cli>(
        "strive-harvester-pipeline-zipgate",
        new Dictionary<string, string>
        {
            { "Worker:Path", zipsPath },
            { "Worker:Overwrite", "true" },
        },
        [rabbit, redis],
        [pipelineMigrator, indexMigrator]
    );

// Unzip
var pipelineUnzipper =
    builder.AddDependentResourceWithSequence<Projects.Fip_Strive_Harvester_Pipeline_Unzipper_Cli>(
        "strive-harvester-pipeline-unzipper",
        new Dictionary<string, string>
        {
            { "Worker:Path", filesPath },
            { "Worker:Overwrite", "true" },
        },
        [rabbit],
        [pipelineMigrator, indexMigrator]
    );

// Scan unique files
var pipelineScanner =
    builder.AddDependentResourceWithSequence<Projects.Fip_Strive_Harvester_Pipeline_Scanner_Cli>(
        "strive-harvester-pipeline-scanner",
        [rabbit, redis],
        [pipelineMigrator, indexMigrator]
    );

var pipelineClassifier =
    builder.AddDependentResourceWithSequence<Projects.Fip_Strive_Harvester_Pipeline_Classify_Cli>(
        "strive-harvester-pipeline-classify",
        new Dictionary<string, string> { { "Worker:Path", filesPath } },
        [rabbit, redis],
        [pipelineMigrator, indexMigrator]
    );

// Synchronizer
// Sync redis indexes to postgres
var indexingSyncer = builder.AddDependentResource<Projects.Fip_Strive_Harvester_Indexing_Sync_Cli>(
    "strive-harvester-indexing-syncer",
    [redis, harvesterDatabase]
);

// Webapps
var harvesterWeb = builder
    .AddProject<Projects.Fip_Strive_Harvester_Web>("strive-harvester-webapp")
    .WithHttpHealthCheck("/health")
    .WithHttpHealthCheck("/health/queue")
    .WithReference(harvesterDatabase)
    .WithReference(rabbit)
    .WithReference(redis)
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
