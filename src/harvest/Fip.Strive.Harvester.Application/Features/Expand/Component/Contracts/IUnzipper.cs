using Fip.Strive.Harvester.Application.Features.Expand.Models;

namespace Fip.Strive.Harvester.Application.Features.Expand.Component.Contracts;

public interface IUnzipper
{
    /// <summary>
    /// Extracts the contents of a zip file into the target directory.
    /// </summary>
    /// <param name="work">State of the current work item.</param>
    /// <param name="overwrite">If files in the output directory should be overwritten.</param>
    /// <param name="ct">The cancellation token to stop the operation.</param>
    /// <returns>
    /// Updated state containing the output directory.
    /// </returns>
    WorkItem Extract(WorkItem work, bool overwrite = false, CancellationToken ct = default);
}
