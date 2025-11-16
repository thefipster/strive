using Fip.Strive.Core.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

namespace Fip.Strive.Harvester.Pipeline.Core;

public static class ConfigureRabbit
{
    public static void AddRabbit(
        this HostApplicationBuilder builder,
        string connectionStringName = "rabbitmq"
    )
    {
        builder.Services.AddSingleton<IConnectionFactory>(_ =>
        {
            var rabbit = builder.Configuration.GetConnectionString(connectionStringName);

            if (string.IsNullOrWhiteSpace(rabbit))
                throw new ConfigurationException("RabbitMQ connection string is missing.");

            return new ConnectionFactory { Uri = new Uri(rabbit) };
        });
    }
}
