using Fip.Strive.Core.Domain.Exceptions;
using Fip.Strive.Harvester.Pipeline.Migrator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

Console.WriteLine("Harvester Pipeline - Migrator starting...");

var builder = Host.CreateApplicationBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddSingleton<IConnectionFactory>(_ =>
{
    var rabbit = configuration.GetConnectionString("rabbitmq");

    if (string.IsNullOrWhiteSpace(rabbit))
        throw new ConfigurationException("RabbitMQ connection string is missing.");

    return new ConnectionFactory { Uri = new Uri(rabbit) };
});

builder.Services.AddScoped<Migrator>();

var app = builder.Build();

var migrator = app.Services.GetRequiredService<Migrator>();
await migrator.ExecuteAsync();

Console.WriteLine("Harvester Pipeline - Migrator exited.");
