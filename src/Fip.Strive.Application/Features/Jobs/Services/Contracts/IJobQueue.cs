namespace Fip.Strive.Application.Features.Jobs.Services.Contracts;

public interface IJobQueue
{
    /// <summary>
    /// Upserts the work unit to <c>Pending</c> and wakes the pump. Enqueueing a unit that already
    /// exists resets it rather than adding a row.
    /// </summary>
    /// <param name="payload">
    /// Serialized to JSON. Omit it to keep whatever is already stored — which is what a retry from
    /// the UI does, having no payload of its own to supply.
    /// </param>
    /// <returns>The unit's id, whether it was created now or already existed.</returns>
    Task<Guid> EnqueueAsync(
        string kind,
        string targetKey,
        object? payload = null,
        CancellationToken cancellationToken = default
    );
}
