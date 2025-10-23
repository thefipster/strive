using Fip.Strive.Harvester.Application.Core.Queue.Models;

namespace Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;

public interface IQueueWorkerFactory
{
    QueueRunner CreateRunner(QueueMetrics metrics);
    QueueReporter CreateReporter(QueueMetrics metrics);
}
