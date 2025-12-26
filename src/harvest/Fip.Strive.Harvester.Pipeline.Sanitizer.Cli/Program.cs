using Fip.Strive.Harvester.Application.Core.PubSub;
using Fip.Strive.Harvester.Application.Core.PubSub.Contracts;
using Fip.Strive.Harvester.Application.Infrastructure;
using Fip.Strive.Harvester.Application.Infrastructure.Pipeline;
using Fip.Strive.Harvester.Pipeline.Sanitizer.Cli;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Harvester Pipeline - Sanitizer init");

var builder = Host.CreateApplicationBuilder(args);
var configuration = builder.Configuration;

builder.AddServiceDefaults();

builder.Services.AddRabbit(configuration);
builder.Services.AddPostgres(configuration).WithPipelineContext();

builder.Services.AddScoped<ISubClient, SubClient>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

Console.WriteLine("Harvester Pipeline - Sanitizer start.");

await host.RunAsync();

Console.WriteLine("Harvester Pipeline - Sanitizer exit.");
