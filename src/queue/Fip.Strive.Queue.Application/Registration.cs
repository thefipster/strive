using System.Diagnostics.CodeAnalysis;
using Fip.Strive.Queue.Application.Components;
using Fip.Strive.Queue.Application.Components.Contracts;
using Fip.Strive.Queue.Application.Contexts;
using Fip.Strive.Queue.Application.Contracts;
using Fip.Strive.Queue.Application.Health;
using Fip.Strive.Queue.Application.Repositories;
using Fip.Strive.Queue.Application.Repositories.Contracts;
using Fip.Strive.Queue.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Queue.Application;

[ExcludeFromCodeCoverage]
public static class Registration
{
    public static void AddQueueFeature<TApp>(this IServiceCollection services)
    {
        services.Scan(scan =>
            scan.FromAssemblyOf<TApp>()
                .AddClasses(classes => classes.AssignableTo<ISignalQueueWorker>())
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        services.AddSingleton<SignalQueueContext>();

        services.AddSingleton<QueuedHostedService>();
        services.AddHostedService(sp => sp.GetRequiredService<QueuedHostedService>());

        services.AddSingleton<ISignalQueue, LiteDbSignalQueue>();

        services.AddSingleton<IQueueWorkerFactory, QueueWorkerFactory>();

        services.AddSingleton<IJobControl, LiteDbJobControl>();

        services.AddScoped<IJobReader, LiteDbJobReader>();

        services.AddScoped<IJobDeleter, LiteDbJobDeleter>();

        services
            .AddHealthChecks()
            .AddCheck<WorkerHealthCheck>("Queue_Workers", tags: new[] { "queue", "workers" })
            .AddCheck<JobHealthCheck>("Queue_Storage", tags: new[] { "queue", "storage" });
    }
}
