using Fip.Strive.Harvester.Application.Core.Queue.Components;
using Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;
using Fip.Strive.Harvester.Application.Core.Queue.Contracts;
using Fip.Strive.Harvester.Application.Core.Queue.Health;
using Fip.Strive.Harvester.Application.Core.Queue.Repositories;
using Fip.Strive.Harvester.Application.Core.Queue.Repositories.Contracts;
using Fip.Strive.Harvester.Application.Core.Queue.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Harvester.Application.Core.Queue;

public static class Registration
{
    public static void AddQueueFeature(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Scan(scan =>
            scan.FromAssembliesOf(typeof(ISignalQueueWorker))
                .AddClasses(classes => classes.AssignableTo<ISignalQueueWorker>())
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        services.AddSingleton<QueuedHostedService>();
        services.AddHostedService(sp => sp.GetRequiredService<QueuedHostedService>());

        services.AddSingleton<ISignalQueue, LiteDbSignalQueue>();

        services.AddSingleton<IQueueWorkerFactory, QueueWorkerFactory>();

        services.AddSingleton<IJobControl, LiteDbJobControl>();

        services.AddScoped<IJobReader, LiteDbJobReader>();

        services.AddScoped<IJobDeleter, LiteDbJobDeleter>();

        services
            .AddHealthChecks()
            .AddCheck<WorkerHealthCheck>("Queue_Workers", tags: new[] { "queue", "workers" });
        services
            .AddHealthChecks()
            .AddCheck<JobHealthCheck>("Queue_Storage", tags: new[] { "queue", "storage" });
    }
}
