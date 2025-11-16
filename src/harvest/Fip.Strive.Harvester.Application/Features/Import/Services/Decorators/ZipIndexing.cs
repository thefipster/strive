using Fip.Strive.Harvester.Application.Features.Import.Services.Contracts;
using Fip.Strive.Harvester.Domain.Signals;
using Fip.Strive.Indexing.Domain.Models;
using Fip.Strive.Indexing.Storage.Contracts;
using Microsoft.Extensions.Logging;

namespace Fip.Strive.Harvester.Application.Features.Import.Services.Decorators;

public class ZipIndexing(
    IImportService component,
    IIndexerV2<ZipIndexV2, string> indexer,
    ILogger<ZipIndexing> logger
) : IImportService
{
    public async Task<string?> MoveZipAsync(UploadSignal signal, CancellationToken ct = default)
    {
        if (await indexer.ExistsAsync(signal.Hash))
            return DeleteUploadedFile(signal.Filepath);

        var path = await component.MoveZipAsync(signal, ct);
        await CreateIndexAsync(signal, path!);

        return path;
    }

    private string? DeleteUploadedFile(string filepath)
    {
        logger.LogInformation(
            "File {UploadFile} is already imported and will be deleted.",
            Path.GetFileName(filepath)
        );

        File.Delete(filepath);

        return null;
    }

    private async Task CreateIndexAsync(UploadSignal signal, string filepath)
    {
        var index = FileIndexV2.Create(filepath, signal.Hash);
        await indexer.SetAsync(index);
    }
}
