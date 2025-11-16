using Fip.Strive.Harvester.Pipeline.Core;
using Fip.Strive.Harvester.Pipeline.Unzipper.Cli;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Harvester Pipeline - Unzipper init");

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.AddRabbit();

builder.AddOptions();
builder.AddServices();

var host = builder.Build();

Console.WriteLine("Harvester Pipeline - Unzipper start.");

await host.RunAsync();

Console.WriteLine("Harvester Pipeline - Unzipper exit.");
