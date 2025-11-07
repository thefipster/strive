using System.Text.Json;
using Fip.Strive.Queue.Application.Contracts;
using Fip.Strive.Queue.Domain.Exceptions;
using Fip.Strive.Queue.Domain.Models;

namespace Fip.Strive.Queue.Application.Components;

public abstract class QueueWorker(int type) : ISignalQueueWorker
{
    public int Type { get; } = type;

    public abstract Task ProcessAsync(JobDetails job, CancellationToken ct);

    protected TSignal GetSafePayload<TSignal>(JobDetails job)
    {
        var payload = job.Payload ?? throw new InvalidSignalException(job, "Payload is null.");

        var signal =
            JsonSerializer.Deserialize<TSignal>(payload)
            ?? throw new InvalidSignalException(job, "Can't read payload.");

        return signal;
    }
}
