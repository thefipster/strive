using TheFipster.ActivityAggregator.Api.Features.Scan.Components.Contracts;
using TheFipster.ActivityAggregator.Api.Features.Scan.Services.Contracts;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Features.Scan.Services;

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
