using Fip.Strive.Harvester.Pipeline.Core;
using Fip.Strive.Harvester.Pipeline.Scanner.Cli;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Harvester Pipeline - Scanner init");

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.AddRabbit();
builder.AddRedis();

builder.AddOptions();
builder.AddServices();

var host = builder.Build();

Console.WriteLine("Harvester Pipeline - Scanner start.");

await host.RunAsync();

Console.WriteLine("Harvester Pipeline - Scanner exit.");
