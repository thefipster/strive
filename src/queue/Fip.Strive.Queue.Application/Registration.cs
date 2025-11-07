using System.Diagnostics.CodeAnalysis;
using Fip.Strive.Queue.Application.Components;
using Fip.Strive.Queue.Application.Components.Contracts;
using Fip.Strive.Queue.Application.Contracts;
using Fip.Strive.Queue.Application.Health;
using Fip.Strive.Queue.Application.Services;
using Fip.Strive.Queue.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Queue.Application;

[ExcludeFromCodeCoverage]
public static class Registration
{
    public static QueueFeatureBuilder AddQueueFeature<TApp>(this IServiceCollection services)
    {
        var storageProvider = new QueueFeatureBuilder(services);

        services.Scan(scan =>
            scan.FromAssemblyOf<TApp>()
                .AddClasses(classes => classes.AssignableTo<ISignalQueueWorker>())
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        services.AddSingleton<QueuedHostedService>();
        services.AddHostedService(sp => sp.GetRequiredService<QueuedHostedService>());

        services.AddSingleton<ISignalQueue, SignalQueue>();

        services.AddSingleton<IQueueWorkerFactory, QueueWorkerFactory>();

        services
            .AddHealthChecks()
            .AddCheck<WorkerHealthCheck>("Queue_Workers", tags: new[] { "queue", "workers" });

        return storageProvider;
    }
}
