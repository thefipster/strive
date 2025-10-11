using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Api.Features.Core.Components.Contracts;
using TheFipster.ActivityAggregator.Domain.Configs;

namespace TheFipster.ActivityAggregator.Api.Features.Batch.Mediators;

public class BatchAction(
    IOptions<ApiConfig> config,
    IBackgroundTaskQueue tasks,
    IBatchService batcher
) : IBatchAction
{
    public void Batch()
    {
        var convergencePath = config.Value.ConvergeDirectoryPath;
        if (!string.IsNullOrWhiteSpace(convergencePath))
        {
            tasks.QueueBackgroundWorkItem(async ct =>
                await batcher.CombineFilesAsync(convergencePath, ct)
            );
        }
    }
}
