using Fip.Strive.Harvester.Application.Infrastructure;
using Fip.Strive.Harvester.Pipeline.Unzipper.Cli;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Harvester Pipeline - Unzipper init");

var builder = Host.CreateApplicationBuilder(args);
var configuration = builder.Configuration;

builder.AddServiceDefaults();

builder.Services.AddRabbit(configuration);

builder.AddOptions();
builder.AddServices();

var host = builder.Build();

Console.WriteLine("Harvester Pipeline - Unzipper start.");

await host.RunAsync();

Console.WriteLine("Harvester Pipeline - Unzipper exit.");
