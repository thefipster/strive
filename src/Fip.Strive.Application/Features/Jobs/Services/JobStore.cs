using Fip.Strive.Application.Features.Jobs.Models;
using Fip.Strive.Application.Features.Jobs.Services.Contracts;
using Fip.Strive.Application.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fip.Strive.Application.Features.Jobs.Services;

public sealed class JobStore(
    StriveContext context,
    TimeProvider timeProvider,
    ILogger<JobStore> logger
) : IJobStore
{
    /// <summary>
    /// One statement, so the select and the update cannot be separated by another claimer.
    /// <c>SKIP LOCKED</c> is what makes concurrent pumps take disjoint sets instead of blocking on
    /// each other — and the reason this is raw SQL rather than EF.
    /// </summary>
    /// <remarks>
    /// Column identifiers are quoted because this repository maps table names to snake_case and
    /// leaves columns as their property names; Postgres would fold an unquoted <c>State</c> to
    /// <c>state</c> and find nothing.
    /// </remarks>
    private const string ClaimSql = """
        UPDATE jobs
        SET "State" = 'Running', "StartedUtc" = {0}, "Attempts" = "Attempts" + 1
        WHERE "Id" IN (
            SELECT "Id" FROM jobs
            WHERE "State" = 'Pending'
            ORDER BY "EnqueuedUtc"
            LIMIT {1}
            FOR UPDATE SKIP LOCKED
        )
        RETURNING "Id" AS "Value"
        """;

    public async Task<IReadOnlyList<Guid>> ClaimAsync(int max, CancellationToken cancellationToken)
    {
        if (max <= 0)
            return [];

        return await context
            .Database.SqlQueryRaw<Guid>(ClaimSql, timeProvider.GetUtcNow(), max)
            .ToListAsync(cancellationToken);
    }

    public Task<Job?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        context.Jobs.AsNoTracking().FirstOrDefaultAsync(job => job.Id == id, cancellationToken);

    public Task CompleteAsync(Guid id, CancellationToken cancellationToken) =>
        context
            .Jobs.Where(job => job.Id == id)
            .ExecuteUpdateAsync(
                setters =>
                    setters
                        .SetProperty(job => job.State, JobState.Succeeded)
                        .SetProperty(job => job.FinishedUtc, timeProvider.GetUtcNow())
                        .SetProperty(job => job.Error, (string?)null),
                cancellationToken
            );

    public async Task FailAsync(Guid id, string error, CancellationToken cancellationToken)
    {
        await context
            .Jobs.Where(job => job.Id == id)
            .ExecuteUpdateAsync(
                setters =>
                    setters
                        .SetProperty(job => job.State, JobState.Failed)
                        .SetProperty(job => job.FinishedUtc, timeProvider.GetUtcNow())
                        .SetProperty(job => job.Error, Truncate(error)),
                cancellationToken
            );

        logger.LogError("Job {JobId} failed: {Error}", id, error);
    }

    public Task ReleaseAsync(Guid id, CancellationToken cancellationToken) =>
        context
            .Jobs.Where(job => job.Id == id)
            .ExecuteUpdateAsync(
                setters =>
                    setters
                        .SetProperty(job => job.State, JobState.Pending)
                        .SetProperty(job => job.StartedUtc, (DateTimeOffset?)null),
                cancellationToken
            );

    public Task SaveProgressAsync(
        Guid id,
        JobProgress progress,
        CancellationToken cancellationToken
    ) =>
        context
            .Jobs.Where(job => job.Id == id)
            .ExecuteUpdateAsync(
                setters =>
                    setters
                        .SetProperty(job => job.ProgressCurrent, progress.Current)
                        .SetProperty(job => job.ProgressTotal, progress.Total)
                        .SetProperty(job => job.ProgressNote, progress.Note),
                cancellationToken
            );

    public async Task<int> RecoverInterruptedAsync(CancellationToken cancellationToken)
    {
        // Attempts is deliberately untouched. Only one process runs, so a Running row here was
        // killed rather than tried — charging it an attempt would turn a restart into a permanent
        // failure under the one-attempt retry policy.
        var recovered = await context
            .Jobs.Where(job => job.State == JobState.Running || job.State == JobState.Stale)
            .ExecuteUpdateAsync(
                setters =>
                    setters
                        .SetProperty(job => job.State, JobState.Pending)
                        .SetProperty(job => job.StartedUtc, (DateTimeOffset?)null),
                cancellationToken
            );

        if (recovered > 0)
            logger.LogInformation("Re-queued {Count} interrupted or stale jobs", recovered);

        return recovered;
    }

    /// <summary>
    /// Exception messages are unbounded and this one is only ever read by a human on the jobs
    /// page. The full detail is in the log next to it.
    /// </summary>
    private static string Truncate(string error) => error.Length <= 2000 ? error : error[..2000];
}
