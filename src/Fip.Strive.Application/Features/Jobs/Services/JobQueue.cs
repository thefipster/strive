using System.Text.Json;
using Fip.Strive.Application.Features.Jobs.Models;
using Fip.Strive.Application.Features.Jobs.Services.Contracts;
using Fip.Strive.Application.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fip.Strive.Application.Features.Jobs.Services;

public sealed class JobQueue(
    StriveContext context,
    IJobRegistry registry,
    JobSignal signal,
    TimeProvider timeProvider,
    ILogger<JobQueue> logger
) : IJobQueue
{
    private static readonly JsonSerializerOptions PayloadOptions = new(JsonSerializerDefaults.Web);

    public async Task<Guid> EnqueueAsync(
        string kind,
        string targetKey,
        object? payload = null,
        CancellationToken cancellationToken = default
    )
    {
        // Resolved before anything is written, so an unregistered kind fails at the call site
        // rather than becoming a row nothing can ever claim.
        var component = registry.Resolve(kind);
        var now = timeProvider.GetUtcNow();
        var json = payload is null ? null : JsonSerializer.Serialize(payload, PayloadOptions);

        var existing = await context.Jobs.FirstOrDefaultAsync(
            job => job.Kind == kind && job.TargetKey == targetKey,
            cancellationToken
        );

        if (existing is not null)
        {
            existing.ComponentId = component.ComponentId;
            existing.ComponentVersion = component.Version;
            existing.State = JobState.Pending;

            // Only when the caller brought one. A retry has no payload to give, and overwriting
            // with null would lose the handler's only input.
            if (json is not null)
                existing.Payload = json;

            // The previous run's outcome is not this run's. Leaving it would have the jobs page
            // showing an error against a job that is queued.
            existing.Error = null;
            existing.ProgressCurrent = null;
            existing.ProgressTotal = null;
            existing.ProgressNote = null;
            existing.EnqueuedUtc = now;
            existing.StartedUtc = null;
            existing.FinishedUtc = null;

            await context.SaveChangesAsync(cancellationToken);
            signal.Set();

            logger.LogInformation("Re-queued {Kind} job for {TargetKey}", kind, targetKey);
            return existing.Id;
        }

        var job = new Job
        {
            Id = Guid.CreateVersion7(),
            Kind = kind,
            TargetKey = targetKey,
            ComponentId = component.ComponentId,
            ComponentVersion = component.Version,
            State = JobState.Pending,
            Payload = json,
            EnqueuedUtc = now,
        };

        context.Jobs.Add(job);
        await context.SaveChangesAsync(cancellationToken);

        // Signalled only after the commit, so the pump can never wake for a row it cannot see.
        signal.Set();

        logger.LogInformation("Queued {Kind} job {JobId} for {TargetKey}", kind, job.Id, targetKey);
        return job.Id;
    }
}
