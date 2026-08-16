namespace Fip.Strive.Application.Features.Jobs.Models;

/// <summary>What an enqueue did to the work unit it names.</summary>
public enum EnqueueOutcome
{
    /// <summary>The unit did not exist and was created.</summary>
    Queued,

    /// <summary>The unit existed in a settled state and was reset to <c>Pending</c>.</summary>
    Requeued,

    /// <summary>
    /// The unit was already running and was left exactly as it was. Resetting a claimed row to
    /// <c>Pending</c> is what lets the pump hand the same unit to a second worker while the first
    /// is still inside its handler.
    /// </summary>
    AlreadyRunning,
}

/// <param name="Outcome">
/// Tells the caller whether the payload it supplied is the one now stored. It is not, for
/// <see cref="EnqueueOutcome.AlreadyRunning"/> — which is how the import page knows that the run
/// already under way owns the archive and its own upload was redundant.
/// </param>
public sealed record EnqueueResult(Guid JobId, EnqueueOutcome Outcome);
