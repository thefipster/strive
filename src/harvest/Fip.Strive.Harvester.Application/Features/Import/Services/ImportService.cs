using Fip.Strive.Harvester.Application.Features.Import.Models;
using Fip.Strive.Harvester.Application.Features.Import.Services.Contracts;
using Fip.Strive.Harvester.Domain.Signals;
using Fip.Strive.Indexing.Application.Features.Contracts;
using Fip.Strive.Indexing.Domain;
using Microsoft.Extensions.Logging;

namespace Fip.Strive.Harvester.Application.Features.Import.Services;

public class ImportService(
    IIndexer<ZipIndex, string> indexer,
    IZipFileAccess fileAccess,
    ILogger<ImportService> logger
) : IImportService
{
    public async Task<WorkItem> ProcessUploadAsync(UploadSignal signal, CancellationToken ct)
    {
        var work = WorkItem.FromSignal(signal);

        work.Index = await indexer.FindAsync(work.Signal.Hash);
        if (FileIsIndexed(work))
            return await RemoveAlreadyKnownFileAsync(work);

        if (work.Index == null)
            work.ImportedPath = fileAccess.Import(work.Signal.Filepath);

        var index = work.ToIndex();
        await indexer.UpsertAsync(index);

        return work;
    }

    private bool FileIsIndexed(WorkItem work)
    {
        return work.Index != null && work.Index.Files.Any(x => x.FileName == work.Filename);
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
