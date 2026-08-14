using Fip.Strive.Application.Features.Jobs.Models;

namespace Fip.Strive.Application.Features.Jobs.Services.Contracts;

/// <summary>
/// The runner's persistence surface. Separate from <see cref="IJobQueue"/> because enqueueing is
/// something the app does and claiming is something only the runner does.
/// </summary>
public interface IJobStore
{
    /// <summary>
    /// Atomically claims up to <paramref name="max"/> pending jobs, marking them running. Returns
    /// the ids actually claimed.
    /// </summary>
    Task<IReadOnlyList<Guid>> ClaimAsync(int max, CancellationToken cancellationToken);

    Task<Job?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task CompleteAsync(Guid id, CancellationToken cancellationToken);

    Task FailAsync(Guid id, string error, CancellationToken cancellationToken);

    /// <summary>
    /// Returns a claimed job to <c>Pending</c> without recording a failure. Used when shutdown
    /// interrupts a handler — the work was not attempted and lost, it was stopped.
    /// </summary>
    Task ReleaseAsync(Guid id, CancellationToken cancellationToken);

    Task SaveProgressAsync(Guid id, JobProgress progress, CancellationToken cancellationToken);

    /// <summary>
    /// Re-queues everything left <c>Running</c> by a previous process, plus anything marked
    /// <c>Stale</c>. Returns how many rows were affected.
    /// </summary>
    Task<int> RecoverInterruptedAsync(CancellationToken cancellationToken);
}
