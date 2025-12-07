using Fip.Strive.Core.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using StackExchange.Redis;

namespace Fip.Strive.Harvester.Application.Infrastructure;

public static class ConfigureProviders
{
    public static ContextExtension AddPostgres(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connection = configuration.GetConnectionString("strive-harvester-database");

        if (string.IsNullOrWhiteSpace(connection))
            throw new ConfigurationException("Postgres connection string is missing.");

        return new ContextExtension(services, connection);
    }

    public static void AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var redisCs = configuration.GetConnectionString("redis");

            if (string.IsNullOrWhiteSpace(redisCs))
                throw new ConfigurationException("Redis connection string is missing.");

            return ConnectionMultiplexer.Connect(redisCs);
        });
    }

    public static void AddRabbit(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IConnectionFactory>(_ =>
        {
            var rabbit = configuration.GetConnectionString("rabbitmq");

            if (string.IsNullOrWhiteSpace(rabbit))
                throw new ConfigurationException("RabbitMQ connection string is missing.");

            return new ConnectionFactory { Uri = new Uri(rabbit) };
        });
    }
}
