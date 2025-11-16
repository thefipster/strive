using Fip.Strive.Harvester.Indexing.Sync.Cli.Setup;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Harvester Indexing - Sync init");

var builder = Host.CreateApplicationBuilder(args);

builder.AddDefaults();

builder.AddPostgres();
builder.AddRedis();

builder.AddServices();

var host = builder.Build();

Console.WriteLine("Harvester Indexing - Sync start.");

await host.RunAsync();

Console.WriteLine("Harvester Indexing - Sync exit.");
