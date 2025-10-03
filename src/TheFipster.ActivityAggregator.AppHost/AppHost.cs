var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.TheFipster_ActivityAggregator_Api>("api");

builder.AddProject<Projects.TheFipster_ActivityAggregator_Web>("blazor").WithReference(api);

builder.Build().Run();
