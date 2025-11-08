using System.Diagnostics.CodeAnalysis;
using Fip.Strive.Queue.Domain;
using Fip.Strive.Queue.Storage.Contracts;
using Fip.Strive.Queue.Storage.Lite.Contexts;
using Fip.Strive.Queue.Storage.Lite.Health;
using Fip.Strive.Queue.Storage.Lite.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Queue.Storage.Lite;

[ExcludeFromCodeCoverage]
public static class Registration
{
    public static QueueFeatureBuilder WithLiteDbStorage(this QueueFeatureBuilder builder)
    {
        var services = builder.Services;

        services.AddSingleton<LiteQueueContext>();

        services.AddSingleton<IJobControl, LiteJobControl>();
        services.AddScoped<IJobReader, LiteJobReader>();
        services.AddScoped<IJobDeleter, LiteJobDeleter>();

        services
            .AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>(
                "Queue_LiteDb_Storage",
                tags: new[] { "queue", "storage", "litedb" }
            );

        return builder;
    }
}
