using Fip.Strive.Harvester.Pipeline.Assimilate.Cli;
using Fip.Strive.Harvester.Pipeline.Core;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Harvester Pipeline - Assimilate init");

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.AddRabbit();
builder.AddRedis();

builder.AddOptions();
builder.AddServices();

var host = builder.Build();

Console.WriteLine("Harvester Pipeline - Assimilate start.");

await host.RunAsync();

Console.WriteLine("Harvester Pipeline - Assimilate exit.");
