using Fip.Strive.Queue.Domain.Models;

namespace Fip.Strive.Queue.Application.Tasks.Contracts;

public interface IQueueWorker
{
    int Type { get; }
    Task ProcessAsync(JobDetails job, CancellationToken ct);
}
