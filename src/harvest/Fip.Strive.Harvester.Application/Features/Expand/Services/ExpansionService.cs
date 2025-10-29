using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Harvester.Application.Features.Expand.Component.Contracts;
using Fip.Strive.Harvester.Application.Features.Expand.Models;
using Fip.Strive.Harvester.Application.Features.Expand.Services.Contracts;

namespace Fip.Strive.Harvester.Application.Features.Expand.Services;

public class ExpansionService(IZipExtractor zipExtractor, IZipEnumerator scanner)
    : IExpansionService
{
    public async Task<WorkItem> UnpackZipFileAsync(ImportSignal signal, CancellationToken ct)
    {
        var workItem = WorkItem.FromSignal(signal);

        workItem = zipExtractor.Expand(workItem, false, ct);
        await scanner.ExploreFolderAsync(workItem, ct);

        return workItem;
    }
}
