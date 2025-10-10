using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Api.Components.Contracts;
using TheFipster.ActivityAggregator.Api.Mediators.Scan.Contracts;
using TheFipster.ActivityAggregator.Api.Services.Contracts;
using TheFipster.ActivityAggregator.Domain.Configs;

namespace TheFipster.ActivityAggregator.Api.Mediators.Scan;

public class ScanAction(
    IOptions<ApiConfig> config,
    IScannerService scanner,
    IBackgroundTaskQueue tasks
) : IScanAction
{
    public void TryScan()
    {
        var destinationDirectory = config.Value.UnzipDirectoryPath;
        if (!string.IsNullOrWhiteSpace(destinationDirectory))
        {
            tasks.QueueBackgroundWorkItem(async ct =>
                await scanner.CheckDirectoryAsync(destinationDirectory, ct)
            );
        }
    }
}
