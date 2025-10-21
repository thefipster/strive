using Fip.Strive.Harvester.Application.Core.Queue.Enums;
using Fip.Strive.Harvester.Application.Core.Queue.Models;

namespace Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;

public interface ISignalQueueWorker
{
    SignalTypes Type { get; }
    Task ProcessAsync(JobEntity job, CancellationToken ct);
}
