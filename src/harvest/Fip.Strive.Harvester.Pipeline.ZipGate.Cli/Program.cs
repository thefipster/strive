using Fip.Strive.Harvester.Pipeline.Core;
using Fip.Strive.Harvester.Pipeline.ZipGate.Cli;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Harvester Pipeline - Zip Gate init");

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.AddDefaults();

builder.AddRabbit();
builder.AddRedis();

builder.AddOptions();
builder.AddServices();

var host = builder.Build();

Console.WriteLine("Harvester Pipeline - Zip Gate start.");

await host.RunAsync();

Console.WriteLine("Harvester Pipeline - Zip Gate exit.");
