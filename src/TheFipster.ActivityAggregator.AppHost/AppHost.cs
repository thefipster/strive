using TheFipster.ActivityAggregator.AppHost.Systems;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddStriveProject();

builder.Build().Run();
