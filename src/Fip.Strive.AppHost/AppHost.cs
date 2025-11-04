var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres").WithPgAdmin();

var indexdb = postgres.AddDatabase("strive-harvester-index");

builder
    .AddProject<Projects.Fip_Strive_Portal_Web>("strive-portal-webapp")
    .WithHttpHealthCheck("/health");

builder
    .AddProject<Projects.Fip_Strive_Unifier_Web>("strive-unifier-webapp")
    .WithHttpHealthCheck("/health");

builder
    .AddProject<Projects.Fip_Strive_Harvester_Web>("strive-harvester-webapp")
    .WithReference(indexdb)
    .WithHttpHealthCheck("/health")
    .WithHttpHealthCheck("/health/queue");

builder.Build().Run();
