using System.Diagnostics.CodeAnalysis;
using Fip.Strive.Core.Application.Extensions;
using Fip.Strive.Queue.Domain;
using Fip.Strive.Queue.Storage.Contracts;
using Fip.Strive.Queue.Storage.Postgres.Contexts;
using Fip.Strive.Queue.Storage.Postgres.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Queue.Storage.Postgres;

[ExcludeFromCodeCoverage]
public static class Registration
{
    public static QueueFeatureBuilder WithPostgresStorage(
        this QueueFeatureBuilder builder,
        IConfiguration configuration
    )
    {
        var services = builder.Services;

        services.AddDbContext<PostgresQueueContext>(
            options => options.UseNpgsql(configuration.GetConnectionString("strive-harvester")),
            ServiceLifetime.Transient
        );

        services.AddTransient<IJobControl, PostgresJobControl>();
        services.AddTransient<IJobReader, PostgresJobReader>();
        services.AddTransient<IJobDeleter, PostgresJobDeleter>();

        return builder;
    }
}
