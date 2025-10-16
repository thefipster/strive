using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Api.Setup.Configs;

namespace TheFipster.ActivityAggregator.Api.Features.Scan.Mediators;

public class ScanAction(
    IOptions<ImportConfig> config,
    IScannerService scanner,
    IBackgroundTaskQueue tasks
) : IScanAction
{
    public void Scan()
    {
        var destinationDirectory = config.Value.UnzipDirectoryPath;
        if (!string.IsNullOrWhiteSpace(destinationDirectory))
        {
            tasks.Enqueue(async ct => await scanner.CheckDirectoryAsync(destinationDirectory, ct));
        }
    }
}
