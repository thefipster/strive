using Fip.Strive.Harvester.Application.Infrastructure;
using Fip.Strive.Harvester.Pipeline.ZipGate.Cli;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Harvester Pipeline - Zip Gate init");

var builder = Host.CreateApplicationBuilder(args);
var configuration = builder.Configuration;

builder.AddServiceDefaults();

builder.Services.AddRabbit(configuration);
builder.Services.AddRedis(configuration);

builder.AddOptions();
builder.AddServices();

var host = builder.Build();

Console.WriteLine("Harvester Pipeline - Zip Gate start.");

await host.RunAsync();

Console.WriteLine("Harvester Pipeline - Zip Gate exit.");
