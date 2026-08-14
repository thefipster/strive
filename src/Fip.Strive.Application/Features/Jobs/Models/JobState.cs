namespace Fip.Strive.Application.Features.Jobs.Models;

public enum JobState
{
    Pending,
    Running,
    Succeeded,
    Failed,

    /// <summary>
    /// The component that produced this unit has moved on. Nothing sets this yet — step 3's
    /// invalidation sweep does — but startup recovery already re-queues it.
    /// </summary>
    Stale,
}
