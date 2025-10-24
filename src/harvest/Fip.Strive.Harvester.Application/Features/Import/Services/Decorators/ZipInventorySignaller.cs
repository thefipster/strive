using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;
using Fip.Strive.Harvester.Application.Features.Import.Models;
using Fip.Strive.Harvester.Application.Features.Import.Services.Contracts;

namespace Fip.Strive.Harvester.Application.Features.Import.Services.Decorators;

public class ZipInventorySignaller(IZipInventory component, ISignalQueue queue) : IZipInventory
{
    public async Task<ZipIndex> ImportAsync(UploadSignal signal, CancellationToken ct)
    {
        var index = await component.ImportAsync(signal, ct);

        var filepath = index.Files.First().Key;
        var newSignal = ImportSignal.From(filepath, signal);
        await queue.EnqueueAsync(newSignal, ct);

        return index;
    }
}
