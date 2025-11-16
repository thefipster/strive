using Fip.Strive.Core.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace Fip.Strive.Harvester.Pipeline.Core;

public static class ConfigureRedis
{
    public static void AddRedis(
        this HostApplicationBuilder builder,
        string connectionStringName = "redis"
    )
    {
        builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var redisCs = builder.Configuration.GetConnectionString(connectionStringName);
            if (string.IsNullOrWhiteSpace(redisCs))
                throw new ConfigurationException("Redis connection string is missing.");

            return ConnectionMultiplexer.Connect(redisCs);
        });
    }
}
