using TheFipster.ActivityAggregator.Api.Components.Contracts;
using TheFipster.ActivityAggregator.Api.Services.Contracts;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Services;

public class ScannerService(IIndexer<ZipIndex> zipInventory, IFileScanner fileScanner)
    : IScannerService
{
    public async Task CheckDirectoryAsync(string destinationDirectory, CancellationToken ct)
    {
        var zips = zipInventory.GetAll().ToArray();

        foreach (var zip in zips)
            await ScanZipOutput(zip, ct);
    }

    private async Task ScanZipOutput(ZipIndex zip, CancellationToken ct)
    {
        var files = Directory.GetFiles(zip.OutputPath, "*", SearchOption.AllDirectories);

        foreach (var filepath in files)
            await fileScanner.HandleFile(filepath, zip.Hash, ct);
    }
}
