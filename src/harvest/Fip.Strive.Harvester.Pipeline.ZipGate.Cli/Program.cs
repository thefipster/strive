using Fip.Strive.Core.Application.Features.FileSystem.Services;
using Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;
using Fip.Strive.Core.Domain.Exceptions;
using Fip.Strive.Harvester.Domain;
using Fip.Strive.Harvester.Pipeline.ZipGate.Cli;
using Fip.Strive.Harvester.Pipeline.ZipGate.Cli.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using StackExchange.Redis;

Console.WriteLine("Harvester Pipeline - Zip Gate starting...");

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json");

var configuration = builder.Configuration;

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Logging.AddConsole();

builder.Services.AddSingleton<IConnectionFactory>(sp =>
{
    var rabbit = configuration.GetConnectionString("rabbitmq");

    if (string.IsNullOrWhiteSpace(rabbit))
        throw new ConfigurationException("RabbitMQ connection string is missing.");

    return new ConnectionFactory { Uri = new Uri(rabbit) };
});

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisCs = configuration.GetConnectionString("redis");
    if (string.IsNullOrWhiteSpace(redisCs))
        throw new ConfigurationException("Redis connection string is missing.");

    return ConnectionMultiplexer.Connect(redisCs);
});

builder
    .Services.AddOptions<HarvestConfig>()
    .BindConfiguration(HarvestConfig.ConfigSectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<IPubSubClient, PubSubClient>();
builder.Services.AddSingleton<IHashIndexer, HashIndexer>();
builder.Services.AddSingleton<IPathIndexer, PathIndexer>();
builder.Services.AddSingleton<IFileService, FileService>();
builder.Services.AddSingleton<Service>();
builder.Services.AddSingleton<IMessageProcessor, Worker>();

builder.Services.AddHostedService<Service>();

var host = builder.Build();

Console.WriteLine("Harvester Pipeline - Zip Gate started.");

await host.RunAsync();

Console.WriteLine("Harvester Pipeline - Zip Gate exited.");
