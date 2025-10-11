using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Api.Features.Core.Components.Contracts;
using TheFipster.ActivityAggregator.Domain.Configs;

namespace TheFipster.ActivityAggregator.Api.Features.Scan.Mediators;

public class ScanAction(
    IOptions<ApiConfig> config,
    IScannerService scanner,
    IBackgroundTaskQueue tasks
) : IScanAction
{
    public void Scan()
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
