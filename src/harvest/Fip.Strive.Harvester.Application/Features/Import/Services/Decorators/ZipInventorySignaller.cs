using Fip.Strive.Core.Domain.Schemas.Index.Models;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;
using Fip.Strive.Harvester.Application.Features.Import.Services.Contracts;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Harvester.Application.Features.Import.Services.Decorators;

public class ZipInventorySignaller(
    IZipInventory component,
    ISignalQueue queue,
    IOptions<ImportConfig> config
) : IZipInventory
{
    private string _rootPath = config.Value.Path;

    public async Task<ZipIndex> ImportAsync(UploadSignal signal, CancellationToken ct)
    {
        var index = await component.ImportAsync(signal, ct);

        var filename = index.Files.First().Key;
        var filepath = Path.Combine(_rootPath, filename);
        var newSignal = ImportSignal.From(filepath, signal);
        await queue.EnqueueAsync(newSignal, ct);

        return index;
    }
}
