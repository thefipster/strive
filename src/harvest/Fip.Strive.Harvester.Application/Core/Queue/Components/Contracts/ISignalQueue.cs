using Fip.Strive.Harvester.Application.Core.Queue.Models;

namespace Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;

public interface ISignalQueue
{
    int Count { get; }
    Task EnqueueAsync(Signal signal, CancellationToken ct = default);
    Task<JobEntity?> DequeueAsync(CancellationToken ct = default);
    Task MarkAsSuccessAsync(Guid jobId);
    Task MarkAsFailedAsync(Guid jobId, Exception ex);
}
