using Fip.Strive.Core.Domain.Schemas.Index.Models;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;
using Fip.Strive.Harvester.Application.Features.Import.Services.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Harvester.Application.Features.Import.Services.Decorators;

public class ZipInventorySignaller(
    IZipInventory component,
    ISignalQueue queue,
    IOptions<ImportConfig> config,
    ILogger<ZipInventorySignaller> logger
) : IZipInventory
{
    private string _rootPath = config.Value.Path;

    public async Task<ZipIndex> ImportAsync(UploadSignal signal, CancellationToken ct)
    {
        var index = await component.ImportAsync(signal, ct);
        var filename = index.Files.First().Key;

        if (index.Files.Count == 0)
            return LogImpossibleError(filename, index);

        if (index.Files.Count > 1)
            return LogFileIsKnown(filename, index);

        await EmitSignal(signal, ct, filename);

        return index;
    }

    private async Task EmitSignal(UploadSignal signal, CancellationToken ct, string filename)
    {
        var filepath = Path.Combine(_rootPath, filename);
        var newSignal = ImportSignal.From(filepath, signal);

        await queue.EnqueueAsync(newSignal, ct);
    }

    private ZipIndex LogFileIsKnown(string filepath, ZipIndex index)
    {
        logger.LogInformation("File {ExpandedFile} is already known. Skipping.", filepath);
        return index;
    }

    private ZipIndex LogImpossibleError(string filepath, ZipIndex index)
    {
        logger.LogInformation(
            "This should not happen... I have a filepath {ExpandedFile} "
                + "but the file list in the index is empty... don't ask me mate, "
                + "something upstream must have gone wrong.",
            filepath
        );
        return index;
    }
}
