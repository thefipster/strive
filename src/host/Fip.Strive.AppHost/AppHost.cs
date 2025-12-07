using Fip.Strive.AppHost.Extensions;

const string zipsPath = @"E:\strive\data\imports\zips";
const string filesPath = @"E:\strive\data\imports\files";
const string dataPath = @"E:\strive\data\imports\data";

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
var indexMigrator =
    builder.AddDependentResource<Projects.Fip_Strive_Harvester_Indexing_Migrator_Cli>(
        "strive-harvester-index-migrator",
        [harvesterDatabase]
    );

// Rabbit Pipeline Exchanges
var pipelineMigrator =
    builder.AddDependentResource<Projects.Fip_Strive_Harvester_Pipeline_Migrator_Cli>(
        "strive-harvester-pipeline-migrator",
        [rabbit, harvesterDatabase]
    );

// Cache Priming
var indexPrimer =
    builder.AddDependentResourceWithSequence<Projects.Fip_Strive_Harvester_Indexing_Primer_Cli>(
        "strive-harvester-index-primer",
        [harvesterDatabase, redis],
        [indexMigrator]
    );

// Pipeline
// Import new zips
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

// Scan for new files
var pipelineScanner =
    builder.AddDependentResourceWithSequence<Projects.Fip_Strive_Harvester_Pipeline_Scanner_Cli>(
        "strive-harvester-pipeline-scanner",
        [rabbit, redis],
        [pipelineMigrator, indexMigrator]
    );

// Classify files
var pipelineClassifier =
    builder.AddDependentResourceWithSequence<Projects.Fip_Strive_Harvester_Pipeline_Classify_Cli>(
        "strive-harvester-pipeline-classify",
        new Dictionary<string, string> { { "Worker:Path", filesPath } },
        [rabbit, redis],
        [pipelineMigrator, indexMigrator]
    );

// Extract data from files
var pipelineAssimilator =
    builder.AddDependentResourceWithSequence<Projects.Fip_Strive_Harvester_Pipeline_Assimilate_Cli>(
        "strive-harvester-pipeline-assimilate",
        new Dictionary<string, string> { { "Worker:Path", dataPath } },
        [rabbit, redis],
        [pipelineMigrator, indexMigrator]
    );

// Synchronizer
// Collect redis indexes and insert into postgres
var indexCollector =
    builder.AddDependentResource<Projects.Fip_Strive_Harvester_Indexing_Collector_Cli>(
        "strive-harvester-indexing-collector",
        [redis, harvesterDatabase],
        [indexMigrator]
    );

var pipelineSanitizer =
    builder.AddDependentResource<Projects.Fip_Strive_Harvester_Pipeline_Sanitizer_Cli>(
        "strive-harvester-pipeline-sanitizer",
        [harvesterDatabase, rabbit],
        [pipelineMigrator]
    );

// Webapps
var harvesterWeb = builder
    .AddProject<Projects.Fip_Strive_Harvester_Web>("strive-harvester-webapp")
    .WithReference(harvesterDatabase)
    .WithReference(rabbit)
    .WithReference(redis)
    .WaitForCompletion(pipelineMigrator)
    .WaitForCompletion(indexMigrator);

// var unifierWeb = builder
//     .AddProject<Projects.Fip_Strive_Unifier_Web>("strive-unifier-webapp")
//     .WithHttpHealthCheck("/health");

// var portalWeb = builder
//     .AddProject<Projects.Fip_Strive_Portal_Web>("strive-portal-webapp")
//     .WithHttpHealthCheck("/health")
//     .WithReference(unifierWeb)
//     .WithReference(harvesterWeb);

builder.Build().Run();
