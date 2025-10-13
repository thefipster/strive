using TheFipster.ActivityAggregator.Api.Features.Core.Components.Contracts;
using TheFipster.ActivityAggregator.Domain;

namespace TheFipster.ActivityAggregator.Api.Features.Scan.Services.Decorators;

public class ScannerNotifier(IScannerService component, INotifier notifier) : IScannerService
{
    public async Task CheckDirectoryAsync(string destinationDirectory, CancellationToken ct)
    {
        await notifier.ReportActionAsync(Defaults.Hubs.Importer.Actions.Scan, "File scan started.");

        await component.CheckDirectoryAsync(destinationDirectory, ct);

        await notifier.ReportActionAsync(
            Defaults.Hubs.Importer.Actions.Scan,
            "Finished file scan.",
            true
        );
    }
}
