using Fip.Strive.Harvester.Pipeline.Classify.Cli;
using Fip.Strive.Harvester.Pipeline.Core;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Harvester Pipeline - Classify init");

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.AddRabbit();
builder.AddRedis();

builder.AddOptions();
builder.AddServices();

var host = builder.Build();

Console.WriteLine("Harvester Pipeline - Classify start.");

await host.RunAsync();

Console.WriteLine("Harvester Pipeline - Classify exit.");
