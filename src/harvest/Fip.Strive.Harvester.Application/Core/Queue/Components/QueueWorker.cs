using System.Text.Json;
using Fip.Strive.Core.Domain.Schemas.Queue.Enums;
using Fip.Strive.Core.Domain.Schemas.Queue.Models;
using Fip.Strive.Harvester.Application.Core.Queue.Contracts;
using Fip.Strive.Harvester.Application.Core.Queue.Exceptions;

namespace Fip.Strive.Harvester.Application.Core.Queue.Components;

public abstract class QueueWorker(SignalTypes type) : ISignalQueueWorker
{
    public SignalTypes Type { get; } = type;

    public abstract Task ProcessAsync(JobDetails job, CancellationToken ct);

    protected TSignal GetSafePayload<TSignal>(JobDetails job)
    {
        var payload = job.Payload ?? throw new InvalidJobException(job, "Payload is null.");

        var signal =
            JsonSerializer.Deserialize<TSignal>(payload)
            ?? throw new InvalidJobException(job, "Can't read payload.");

        return signal;
    }
}
