using Fip.Strive.Core.Domain.Schemas.Queue.Enums;
using Fip.Strive.Core.Domain.Schemas.Queue.Models;
using Fip.Strive.Harvester.Application.Core.Queue.Models;

namespace Fip.Strive.Harvester.Application.Core.Queue.Contracts;

public interface ISignalQueueWorker
{
    SignalTypes Type { get; }
    Task ProcessAsync(JobDetails job, CancellationToken ct);
}
