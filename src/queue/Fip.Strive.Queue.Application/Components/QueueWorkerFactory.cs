using Fip.Strive.Queue.Application.Components.Contracts;
using Fip.Strive.Queue.Domain.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Queue.Application.Components;

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
