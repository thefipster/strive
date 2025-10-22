using Fip.Strive.Harvester.Application.Core.Queue.Components;
using Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;
using Fip.Strive.Harvester.Application.Core.Queue.Contracts;
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
        services.Configure<QueueConfig>(configuration.GetSection(QueueConfig.ConfigSectionName));

        services.Scan(scan =>
            scan.FromAssembliesOf(typeof(ISignalQueueWorker))
                .AddClasses(classes => classes.AssignableTo<ISignalQueueWorker>())
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        services.AddHostedService<QueuedHostedService>();

        services.AddSingleton<ISignalQueue, LiteDbSignalQueue>();

        services.AddSingleton<IJobStorage, LiteDbJobStorage>();

        services.AddSingleton<IJobReader, LiteDbJobReader>();
    }
}
