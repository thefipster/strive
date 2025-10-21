var builder = DistributedApplication.CreateBuilder(args);

builder
    .AddProject<Projects.Fip_Strive_Harvester_Web>("strive-harvester-webapp")
    .WithHttpHealthCheck("/health");

builder.Build().Run();
