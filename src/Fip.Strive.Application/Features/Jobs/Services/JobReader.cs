using Fip.Strive.Application.Features.Jobs.Models;
using Fip.Strive.Application.Features.Jobs.Services.Contracts;
using Fip.Strive.Application.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Application.Features.Jobs.Services;

public sealed class JobReader(StriveContext context) : IJobReader
{
    public async Task<JobCounts> GetCountsAsync(CancellationToken cancellationToken = default)
    {
        // One round trip rather than five: every open jobs page re-reads this on every change.
        var counts = await context
            .Jobs.AsNoTracking()
            .GroupBy(job => job.State)
            .Select(group => new { State = group.Key, Count = group.Count() })
            .ToDictionaryAsync(row => row.State, row => row.Count, cancellationToken);

        return new JobCounts(
            counts.GetValueOrDefault(JobState.Pending),
            counts.GetValueOrDefault(JobState.Running),
            counts.GetValueOrDefault(JobState.Succeeded),
            counts.GetValueOrDefault(JobState.Failed),
            counts.GetValueOrDefault(JobState.Stale)
        );
    }

    public async Task<IReadOnlyList<JobRow>> GetJobsAsync(
        int take = 100,
        CancellationToken cancellationToken = default
    ) =>
        await context
            .Jobs.AsNoTracking()
            // Running, then pending, then everything finished — someone opening this page is
            // looking for what is happening now, not for last week's successes.
            .OrderBy(job =>
                job.State == JobState.Running ? 0
                : job.State == JobState.Pending ? 1
                : 2
            )
            .ThenByDescending(job => job.FinishedUtc ?? job.StartedUtc ?? job.EnqueuedUtc)
            .Take(take)
            .Select(job => new JobRow(
                job.Id,
                job.Kind,
                job.TargetKey,
                job.ComponentId,
                job.ComponentVersion,
                job.State,
                job.Attempts,
                job.Error,
                job.ProgressCurrent,
                job.ProgressTotal,
                job.ProgressNote,
                job.EnqueuedUtc,
                job.StartedUtc,
                job.FinishedUtc
            ))
            .ToListAsync(cancellationToken);
}
