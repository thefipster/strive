using Fip.Strive.AppHost.Systems;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddStriveProject();

builder.Build().Run();
