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

var unifierWeb = builder
    .AddProject<Projects.Fip_Strive_Unifier_Web>("strive-unifier-webapp")
    .WithHttpHealthCheck("/health");

var harvesterWeb = builder
    .AddProject<Projects.Fip_Strive_Harvester_Web>("strive-harvester-webapp")
    .WithHttpHealthCheck("/health")
    .WithHttpHealthCheck("/health/queue")
    .WithReference(harvesterDatabase)
    .WaitForCompletion(indexMigrator)
    .WaitForCompletion(queueMigrator);

builder
    .AddProject<Projects.Fip_Strive_Portal_Web>("strive-portal-webapp")
    .WithHttpHealthCheck("/health")
    .WithReference(unifierWeb)
    .WithReference(harvesterWeb);

builder.Build().Run();
