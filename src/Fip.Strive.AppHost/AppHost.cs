var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres").WithPgAdmin();

var harvesterDatabase = postgres.AddDatabase("strive-harvester");

var indexMigrator = builder
    .AddProject<Projects.Fip_Strive_Indexing_Storage_Postgres_Migrator>(
        "strive-harvester-index-migrator"
    )
    .WithReference(harvesterDatabase)
    .WaitFor(harvesterDatabase);

var queueMigrator = builder
    .AddProject<Projects.Fip_Strive_Queue_Storage_Postgres_Migrator>(
        "strive-harvester-queue-migrator"
    )
    .WithReference(harvesterDatabase)
    .WaitFor(harvesterDatabase)
    .WaitFor(indexMigrator);

var redis = builder.AddRedis("redis").WithDataVolume().WithRedisInsight();

var rabbitUser = builder.AddParameter("RabbitUser");
var rabbitPass = builder.AddParameter("RabbitPassword", true);

var rabbit = builder.AddRabbitMQ("rabbitmq", rabbitUser, rabbitPass).WithManagementPlugin();

var unifierWeb = builder
    .AddProject<Projects.Fip_Strive_Unifier_Web>("strive-unifier-webapp")
    .WithHttpHealthCheck("/health");

var harvesterWeb = builder
    .AddProject<Projects.Fip_Strive_Harvester_Web>("strive-harvester-webapp")
    .WithHttpHealthCheck("/health")
    .WithHttpHealthCheck("/health/queue")
    .WithReference(harvesterDatabase)
    .WithReference(rabbit)
    .WaitForCompletion(indexMigrator)
    .WaitForCompletion(queueMigrator);

var portalWeb = builder
    .AddProject<Projects.Fip_Strive_Portal_Web>("strive-portal-webapp")
    .WithHttpHealthCheck("/health")
    .WithReference(unifierWeb)
    .WithReference(harvesterWeb);

var pipelineZipGate = builder
    .AddProject<Projects.Fip_Strive_Harvester_Pipeline_ZipGate_Cli>(
        "strive-harvester-pipeline-zipgate"
    )
    .WithReference(rabbit)
    .WaitForStart(harvesterWeb);

builder.Build().Run();
