namespace Fip.Strive.Application.Features.Jobs.Models;

/// <summary>
/// Everything a handler is given. <paramref name="Progress"/> is throttled on the way to the
/// database, so a handler may report as often as is natural for it.
/// </summary>
public sealed record JobContext(Job Job, IProgress<JobProgress> Progress);
