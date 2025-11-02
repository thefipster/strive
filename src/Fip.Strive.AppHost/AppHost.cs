var builder = DistributedApplication.CreateBuilder(args);

builder
    .AddProject<Projects.Fip_Strive_Portal_Web>("strive-portal-webapp")
    .WithHttpHealthCheck("/health");

builder
    .AddProject<Projects.Fip_Strive_Unifier_Web>("strive-unifier-webapp")
    .WithHttpHealthCheck("/health");

builder
    .AddProject<Projects.Fip_Strive_Harvester_Web>("strive-harvester-webapp")
    .WithHttpHealthCheck("/health")
    .WithHttpHealthCheck("/health/queue");

builder.Build().Run();
