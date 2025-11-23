using Fip.Strive.Core.Domain.Exceptions;
using Fip.Strive.Harvester.Application.Infrastructure.Indexing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace Fip.Strive.Harvester.Indexing.Sync.Cli.Setup;

public static class Setup
{
    public static void AddPostgres(this HostApplicationBuilder builder)
    {
        var connection = builder.Configuration.GetConnectionString("strive-harvester-database");

        if (string.IsNullOrWhiteSpace(connection))
            throw new ConfigurationException("Postgres connection string is missing.");

        builder.Services.AddDbContext<IndexContext>(options => options.UseNpgsql(connection));

        builder.Services.AddDbContextFactory<IndexContext>(options =>
            options.UseNpgsql(connection)
        );
    }

    public static void AddRedis(this HostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redisCs = builder.Configuration.GetConnectionString("redis");
            if (string.IsNullOrWhiteSpace(redisCs))
                throw new ConfigurationException("Redis connection string is missing.");

            return ConnectionMultiplexer.Connect(redisCs);
        });
    }
}
