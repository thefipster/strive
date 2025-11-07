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

        services.AddSingleton<SignalQueueContext>();

        services.AddSingleton<IJobControl, LiteDbJobControl>();

        services.AddScoped<IJobReader, LiteDbJobReader>();

        services.AddScoped<IJobDeleter, LiteDbJobDeleter>();

        services
            .AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("Queue_Storage", tags: new[] { "queue", "storage" });

        return builder;
    }
}
