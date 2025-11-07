using Fip.Strive.Queue.Domain.Models;

namespace Fip.Strive.Queue.Application.Components.Contracts;

public interface IQueueWorkerFactory
{
    QueueRunner CreateRunner(QueueMetrics metrics);
    QueueReporter CreateReporter(QueueMetrics metrics);
}
