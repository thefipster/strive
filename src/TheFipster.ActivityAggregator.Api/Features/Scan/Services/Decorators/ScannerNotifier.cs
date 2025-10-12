using TheFipster.ActivityAggregator.Domain;

namespace TheFipster.ActivityAggregator.Api.Features.Scan.Services.Decorators;

public class ScannerNotifier(IScannerService component, INotifier notifier) : IScannerService
{
    public async Task CheckDirectoryAsync(string destinationDirectory, CancellationToken ct)
    {
        await notifier.ReportActionAsync(Const.Hubs.Importer.Actions.Scan, "File scan started.");
        await component.CheckDirectoryAsync(destinationDirectory, ct);
    }
}
