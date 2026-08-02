using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Api.Setup.Configs;

namespace TheFipster.ActivityAggregator.Api.Features.Assimilate.Mediators;

public class AssimilateAction(
    IOptions<ImportConfig> config,
    IAssimilaterService assimilater,
    IBackgroundTaskQueue tasks
) : IAssimilateAction
{
    public void Assimilate()
    {
        var destinationDirectory = config.Value.UnzipDirectoryPath;
        if (string.IsNullOrWhiteSpace(destinationDirectory))
            ThrowMissingPathException();

        tasks.Enqueue(async ct => await assimilater.ExtractFiles(destinationDirectory, ct));
    }

    private static void ThrowMissingPathException()
    {
        throw new ArgumentException(
            "Converge directory path cannot be empty.",
            nameof(ImportConfig.ConvergeDirectoryPath)
        );
    }
}
