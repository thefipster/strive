using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Api.Setup.Configs;

namespace TheFipster.ActivityAggregator.Api.Features.Batch.Mediators;

public class BatchAction(
    IOptions<ImportConfig> config,
    IBackgroundTaskQueue tasks,
    IBatchService batcher
) : IBatchAction
{
    public void Batch()
    {
        var convergencePath = config.Value.ConvergeDirectoryPath;
        if (!string.IsNullOrWhiteSpace(convergencePath))
        {
            tasks.Enqueue(async ct => await batcher.CombineFilesAsync(convergencePath, ct));
        }
    }
}
