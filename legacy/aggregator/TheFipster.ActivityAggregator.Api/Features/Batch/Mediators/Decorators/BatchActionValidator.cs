using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Api.Setup.Configs;

namespace TheFipster.ActivityAggregator.Api.Features.Batch.Mediators.Decorators;

public class BatchActionValidator(IBatchAction component, IOptions<ImportConfig> config)
    : IBatchAction
{
    public void Batch()
    {
        if (string.IsNullOrWhiteSpace(config.Value.ConvergeDirectoryPath))
            throw new ArgumentException(
                "Convergence path cannot be null or empty",
                nameof(config.Value.ConvergeDirectoryPath)
            );

        component.Batch();
    }
}
