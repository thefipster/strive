using Fip.Strive.Core.Domain.Extensions;
using Fip.Strive.Core.Domain.Schemas.Index.Models;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Harvester.Application.Features.Import.Components.Contracts;
using Fip.Strive.Harvester.Application.Features.Import.Repositories.Contracts;
using Fip.Strive.Harvester.Application.Features.Import.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace Fip.Strive.Harvester.Application.Features.Import.Services;

public class ZipInventory(
    IZipIndexer indexer,
    IZipFileAccess fileAccess,
    ILogger<ZipInventory> logger
) : IZipInventory
{
    public async Task<ZipIndex> ImportAsync(UploadSignal signal, CancellationToken ct)
    {
        var file = new FileInfo(signal.Filepath);
        var hash = await file.HashXx3Async(ct);
        var index = indexer.Get(hash);

        if (FileIsIndexed(index, file))
            return RemoveAlreadyKnownFile(signal, file.Name, index!);

        if (index == null)
            index = ImportNewFile(signal, hash);

        index.AddFile(file.Name);
        indexer.Set(index);

        return index;
    }

    private bool FileIsIndexed(ZipIndex? index, FileInfo file)
    {
        return index != null && index.Files.ContainsKey(file.Name);
    }

    private ZipIndex RemoveAlreadyKnownFile(UploadSignal signal, string filename, ZipIndex index)
    {
        logger.LogInformation(
            "File {UploadFile} is already imported and will be deleted.",
            filename
        );
        File.Delete(signal.Filepath);
        return index;
    }

    private ZipIndex ImportNewFile(UploadSignal signal, string hash)
    {
        fileAccess.Import(signal.Filepath);
        var index = ZipIndex.From(signal, hash);
        return index;
    }
}
