using Fip.Strive.Core.Domain.Exceptions;
using Fip.Strive.Harvester.Pipeline.ZipGate.Cli;
using Fip.Strive.Harvester.Pipeline.ZipGate.Cli.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

Console.WriteLine("Harvester Pipeline - Zip Gate starting...");

var builder = Host.CreateApplicationBuilder(args);
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

builder.Services.AddSingleton<Subscriber>();
builder.Services.AddSingleton<IMessageProcessor, Worker>();

builder.Services.AddHostedService<Subscriber>();

var host = builder.Build();

Console.WriteLine("Harvester Pipeline - Zip Gate started.");

await host.RunAsync();

Console.WriteLine("Harvester Pipeline - Zip Gate exited.");
