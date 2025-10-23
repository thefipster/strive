using Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;
using Fip.Strive.Harvester.Application.Core.Queue.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Harvester.Application.Core.Queue.Components;

public class QueueWorkerFactory(IServiceProvider serviceProvider) : IQueueWorkerFactory
{
    public QueueRunner CreateRunner(QueueMetrics metrics)
    {
        return ActivatorUtilities.CreateInstance<QueueRunner>(serviceProvider, metrics);
    }

    public QueueReporter CreateReporter(QueueMetrics metrics)
    {
        return ActivatorUtilities.CreateInstance<QueueReporter>(serviceProvider, metrics);
    }
}
