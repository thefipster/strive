using Fip.Strive.Core.Domain.Schemas.Index.Models;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;
using Fip.Strive.Harvester.Application.Features.Import.Models;
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
    private readonly string _rootPath = config.Value.Path;

    public async Task<WorkItem> ImportAsync(UploadSignal signal, CancellationToken ct)
    {
        var work = await component.ImportAsync(signal, ct);

        if (work.Skip)
            return WorkAndLogFileAlreadyKnown(work);

        await EmitSignal(signal, ct, work.ImportedPath!);

        return work;
    }

    private async Task EmitSignal(UploadSignal signal, CancellationToken ct, string filename)
    {
        var filepath = Path.Combine(_rootPath, filename);
        var newSignal = ImportSignal.From(filepath, signal);

        await queue.EnqueueAsync(newSignal, ct);
    }

    private WorkItem WorkAndLogFileAlreadyKnown(WorkItem work)
    {
        logger.LogInformation("File {ImportedFile} is already known. Skipping.", work.ImportedPath);
        return work;
    }
}
