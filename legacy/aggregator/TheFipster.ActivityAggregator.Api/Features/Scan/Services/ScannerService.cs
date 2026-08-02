using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Features.Indexing.Components;

namespace TheFipster.ActivityAggregator.Api.Features.Scan.Services;

public class ScannerService(
    IIndexer<ZipIndex> zipInventory,
    IFileScanner fileScanner,
    IBackgroundTaskQueue queue
) : IScannerService
{
    public Task CheckDirectoryAsync(string destinationDirectory, CancellationToken ct)
    {
        var zips = zipInventory.GetAll().ToArray();

        foreach (var zip in zips)
            queue.Enqueue(async _ => await ScanZipOutput(zip));

        return Task.CompletedTask;
    }

    private Task ScanZipOutput(ZipIndex zip)
    {
        var files = Directory.GetFiles(zip.OutputPath, "*", SearchOption.AllDirectories);

        foreach (var filepath in files)
            queue.Enqueue(async ctx => await fileScanner.HandleFile(filepath, zip.Hash, ctx));

        return Task.CompletedTask;
    }
}
