using Fip.Strive.Queue.Domain.Models;

namespace Fip.Strive.Queue.Application.Contracts;

public interface ISignalQueueWorker
{
    int Type { get; }
    Task ProcessAsync(JobDetails job, CancellationToken ct);
}
