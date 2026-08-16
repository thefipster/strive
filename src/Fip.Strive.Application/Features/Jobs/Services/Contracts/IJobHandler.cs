using Fip.Strive.Application.Features.Jobs.Models;

namespace Fip.Strive.Application.Features.Jobs.Services.Contracts;

/// <summary>
/// One kind of work. Implementations are discovered from DI and must be idempotent: startup
/// recovery re-runs anything that was interrupted, so a handler is expected to survive being run
/// twice against the same target.
/// </summary>
public interface IJobHandler
{
    /// <summary>Matches <see cref="Models.Job.Kind"/>. Unique across all handlers.</summary>
    string Kind { get; }

    /// <summary>
    /// The versioned component this handler's work belongs to. Distinct from <see cref="Kind"/>
    /// because later steps run many versioned components under one kind.
    /// </summary>
    string ComponentId { get; }

    /// <summary>Bumping this is what will mark existing units stale, from step 3 onwards.</summary>
    int Version { get; }

    Task ExecuteAsync(JobContext context, CancellationToken cancellationToken);
}
