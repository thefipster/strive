using Fip.Strive.Harvester.Application.Features.Import.Services.Contracts;
using Fip.Strive.Harvester.Domain.Signals;
using Fip.Strive.Queue.Application.Services.Contracts;

namespace Fip.Strive.Harvester.Application.Features.Import.Services.Decorators;

public class ZipEmission(IImportService component, IQueueService queue) : IImportService
{
    public async Task<string?> MoveZipAsync(UploadSignal signal, CancellationToken ct = default)
    {
        var path = await component.MoveZipAsync(signal, ct);

        if (string.IsNullOrWhiteSpace(path))
            return path;

        await EnqueueSignal(signal, path, ct);
        return path;
    }

    private async Task EnqueueSignal(UploadSignal inSignal, string filepath, CancellationToken ct)
    {
        var outSignal = ImportSignal.From(filepath, inSignal);
        await queue.EnqueueAsync(outSignal, ct);
    }
}
