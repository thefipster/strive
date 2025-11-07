var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres").WithPgAdmin();
var indexdb = postgres.AddDatabase("strive-harvester-index");

var indexMigrator = builder
    .AddProject<Projects.Fip_Strive_Indexing_Migrator>("strive-harvester-index-migrator")
    .WithReference(indexdb)
    .WaitFor(indexdb);

var unifierWeb = builder
    .AddProject<Projects.Fip_Strive_Unifier_Web>("strive-unifier-webapp")
    .WithHttpHealthCheck("/health");

var harvesterWeb = builder
    .AddProject<Projects.Fip_Strive_Harvester_Web>("strive-harvester-webapp")
    .WithHttpHealthCheck("/health")
    .WithHttpHealthCheck("/health/queue")
    .WithReference(indexdb)
    .WaitForCompletion(indexMigrator);

var portalWeb = builder
    .AddProject<Projects.Fip_Strive_Portal_Web>("strive-portal-webapp")
    .WithHttpHealthCheck("/health")
    .WithReference(unifierWeb)
    .WithReference(harvesterWeb);

builder.Build().Run();
