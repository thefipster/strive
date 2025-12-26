using Fip.Strive.Harvester.Application.Infrastructure;
using Fip.Strive.Harvester.Application.Infrastructure.Indexing;
using Fip.Strive.Harvester.Indexing.Collector.Cli.Setup;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Harvester Indexing - Sync init");

var builder = Host.CreateApplicationBuilder(args);
var configuration = builder.Configuration;

builder.AddServiceDefaults();

builder.Services.AddPostgres(configuration).WithIndexingContext();
builder.Services.AddRedis(configuration);

builder.AddServices();

var host = builder.Build();

Console.WriteLine("Harvester Indexing - Sync start.");

await host.RunAsync();

Console.WriteLine("Harvester Indexing - Sync exit.");
