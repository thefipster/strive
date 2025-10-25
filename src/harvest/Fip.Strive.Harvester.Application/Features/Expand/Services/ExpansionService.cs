using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Harvester.Application.Features.Expand.Component.Contracts;
using Fip.Strive.Harvester.Application.Features.Expand.Models;
using Fip.Strive.Harvester.Application.Features.Expand.Services.Contracts;

namespace Fip.Strive.Harvester.Application.Features.Expand.Services;

public class ExpansionService(IUnzipper unzipper, IZipEnumerator scanner) : IExpansionService
{
    public async Task UnpackZipFileAsync(ImportSignal signal, CancellationToken ct)
    {
        var workItem = WorkItem.FromSignal(signal);

        workItem = unzipper.Extract(workItem, false, ct);
        await scanner.ExploreFolderAsync(workItem, ct);
    }
}
