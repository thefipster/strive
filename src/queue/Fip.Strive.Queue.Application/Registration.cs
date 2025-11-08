using System.Diagnostics.CodeAnalysis;
using Fip.Strive.Queue.Application.Health;
using Fip.Strive.Queue.Application.Services;
using Fip.Strive.Queue.Application.Services.Contracts;
using Fip.Strive.Queue.Application.Tasks.Contracts;
using Fip.Strive.Queue.Domain;
using Fip.Strive.Queue.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using TaskFactory = Fip.Strive.Queue.Application.Services.TaskFactory;

namespace Fip.Strive.Queue.Application;

[ExcludeFromCodeCoverage]
public static class Registration
{
    public static QueueFeatureBuilder AddQueueFeature<TApp>(this IServiceCollection services)
    {
        services.Scan(scan =>
            scan.FromAssemblyOf<TApp>()
                .AddClasses(classes => classes.AssignableTo<IQueueWorker>())
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        services.AddHostedService<HostedService>();

        services.AddSingleton<QueueMetrics>();
        services.AddSingleton<ITaskFactory, TaskFactory>();
        services.AddSingleton<IQueueService, QueueService>();
        services.AddSingleton<IProcessingService, ProcessingService>();

        services
            .AddHealthChecks()
            .AddCheck<WorkerHealthCheck>("Queue_Workers", tags: ["queue", "workers"]);

        return new QueueFeatureBuilder(services);
    }
}
