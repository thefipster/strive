using Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;
using Fip.Strive.Core.Domain.Schemas.Index.Models;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Harvester.Application.Core.Indexing.Contracts;
using Fip.Strive.Harvester.Application.Features.Import.Components.Contracts;
using Fip.Strive.Harvester.Application.Features.Import.Models;
using Fip.Strive.Harvester.Application.Features.Import.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace Fip.Strive.Harvester.Application.Features.Import.Services;

public class ZipInventory(
    IIndexer<ZipIndex, string> indexer,
    IZipFileAccess fileAccess,
    ILogger<ZipInventory> logger
) : IZipInventory
{
    public async Task<WorkItem> ImportAsync(UploadSignal signal, CancellationToken ct)
    {
        var work = WorkItem.FromSignal(signal);

        work.Index = indexer.Find(work.Signal.Hash);
        if (FileIsIndexed(work))
            return await RemoveAlreadyKnownFileAsync(work);

        if (work.Index == null)
            work.ImportedPath = fileAccess.Import(work.Signal.Filepath);

        var index = work.ToIndex();
        indexer.Upsert(index);

        return work;
    }

    private bool FileIsIndexed(WorkItem work)
    {
        return work.Index != null && work.Index.Files.ContainsKey(work.Filename);
    }

    private Task<WorkItem> RemoveAlreadyKnownFileAsync(WorkItem work)
    {
        logger.LogInformation(
            "File {UploadFile} is already imported and will be deleted.",
            work.Filename
        );

        work.Skip = true;
        File.Delete(work.Signal.Filepath);

        return Task.FromResult(work);
    }
}
