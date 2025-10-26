using Fip.Strive.Harvester.Application.Features.Expand.Models;

namespace Fip.Strive.Harvester.Application.Features.Expand.Component.Contracts;

public interface IZipEnumerator
{
    Task ExploreFolderAsync(WorkItem work, CancellationToken ct);
}
