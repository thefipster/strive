using TheFipster.ActivityAggregator.Api.Features.Core.Components.Contracts;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Features.Scan.Services;

public class ScannerService(
    IIndexer<ZipIndex> zipInventory,
    IFileScanner fileScanner,
    INotifier notifier
) : IScannerService
{
    private int _totalZips;
    private int _processedZips;

    public async Task CheckDirectoryAsync(string destinationDirectory, CancellationToken ct)
    {
        var zips = zipInventory.GetAll().ToArray();
        _totalZips = zips.Length;

        foreach (var zip in zips)
            await ScanZipOutput(zip, ct);
    }

    private async Task ScanZipOutput(ZipIndex zip, CancellationToken ct)
    {
        await ReportProgress(zip);

        var files = Directory.GetFiles(zip.OutputPath, "*", SearchOption.AllDirectories);

        foreach (var filepath in files)
            await fileScanner.HandleFile(filepath, zip.Hash, ct);

        _processedZips++;
    }

    private async Task ReportProgress(ZipIndex zip)
    {
        await notifier.ReportProgressAsync(
            Const.Hubs.Importer.Actions.Scan,
            new FileInfo(zip.ZipPath).Name,
            Math.Round((double)_processedZips / _totalZips, 3)
        );
    }
}
