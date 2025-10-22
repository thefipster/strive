using Fip.Strive.Harvester.Application.Core.Queue.Enums;
using Fip.Strive.Harvester.Application.Core.Queue.Models;

namespace Fip.Strive.Harvester.Application.Core.Queue.Contracts;

public interface ISignalQueueWorker
{
    SignalTypes Type { get; }
    Task ProcessAsync(JobDetails job, CancellationToken ct);
}
