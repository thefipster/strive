using System.Text.Json;
using Fip.Strive.Application.Features.Jobs.Models;
using Fip.Strive.Application.Features.Jobs.Services.Contracts;
using Fip.Strive.Application.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Fip.Strive.Application.Features.Jobs.Services;

public sealed class JobQueue(
    StriveContext context,
    IJobRegistry registry,
    JobSignal signal,
    TimeProvider timeProvider,
    ILogger<JobQueue> logger
) : IJobQueue
{
    /// <summary>
    /// How many times to re-run the upsert after losing the insert race. One is enough for the
    /// race itself — the loser's next read finds the winner's row — and the second exists only so
    /// that a pathological sequence still terminates rather than looping.
    /// </summary>
    private const int MaxAttempts = 3;

    private static readonly JsonSerializerOptions PayloadOptions = new(JsonSerializerDefaults.Web);

    public async Task<EnqueueResult> EnqueueAsync(
        string kind,
        string targetKey,
        object? payload = null,
        CancellationToken cancellationToken = default
    )
    {
        // Resolved before anything is written, so an unregistered kind fails at the call site
        // rather than becoming a row nothing can ever claim.
        var component = registry.Resolve(kind);
        var json = payload is null ? null : JsonSerializer.Serialize(payload, PayloadOptions);

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await UpsertAsync(kind, targetKey, component, json, cancellationToken);
            }
            catch (DbUpdateException exception)
                when (IsDuplicateUnit(exception) && attempt < MaxAttempts)
            {
                // Two callers found no row and both inserted. The unique index on
                // (Kind, TargetKey) makes the outcome safe — one work unit, as intended — but the
                // loser was getting a raw EF exception for something that is not a failure. Drop
                // the row it could not write and go round again; this time the read finds the
                // winner's and takes the update path.
                context.ChangeTracker.Clear();

                logger.LogInformation(
                    "Lost the insert race for the {Kind} unit {TargetKey}; re-reading it",
                    kind,
                    targetKey
                );
            }
        }
    }

    private async Task<EnqueueResult> UpsertAsync(
        string kind,
        string targetKey,
        JobComponent component,
        string? json,
        CancellationToken cancellationToken
    )
    {
        var now = timeProvider.GetUtcNow();

        var existing = await context.Jobs.FirstOrDefaultAsync(
            job => job.Kind == kind && job.TargetKey == targetKey,
            cancellationToken
        );

        if (existing is null)
            return await InsertAsync(kind, targetKey, component, json, now, cancellationToken);

        // A claimed row belongs to the worker holding it. Putting it back to Pending would not
        // interrupt that worker — it would simply let the pump hand the same unit to a second one,
        // and leave the two of them racing to write the outcome of a unit that only ran once as
        // far as anyone watching can tell.
        if (existing.State == JobState.Running)
        {
            logger.LogInformation(
                "The {Kind} unit {TargetKey} is already running; leaving it alone",
                kind,
                targetKey
            );

            return new EnqueueResult(existing.Id, EnqueueOutcome.AlreadyRunning);
        }

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
        return new EnqueueResult(existing.Id, EnqueueOutcome.Requeued);
    }

    private async Task<EnqueueResult> InsertAsync(
        string kind,
        string targetKey,
        JobComponent component,
        string? json,
        DateTimeOffset now,
        CancellationToken cancellationToken
    )
    {
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
        return new EnqueueResult(job.Id, EnqueueOutcome.Queued);
    }

    /// <summary>
    /// Postgres reports a broken unique index as SQLSTATE 23505. The only unique index this
    /// statement can break is the one on (Kind, TargetKey), so the state alone identifies it.
    /// </summary>
    private static bool IsDuplicateUnit(DbUpdateException exception) =>
        exception.InnerException
            is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}
