using Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;
using Fip.Strive.Harvester.Application.Core.PubSub.Contracts;
using Fip.Strive.Harvester.Domain.Defaults;
using Fip.Strive.Harvester.Domain.Signals;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Harvester.Pipeline.Unzipper.Cli.Application;

public class Worker(
    IPubSubClient client,
    IZipService file,
    IOptions<Config> config,
    ILogger<Worker> logger
) : IProcessor
{
    private readonly string _unzipPath = config.Value.Path;
    private readonly bool _overwrite = config.Value.Overwrite;

    private readonly DirectExchange _exchange = HarvestPipelineExchange.New(SignalTypes.ScanSignal);

    public async Task ProcessAsync(string inMessage, CancellationToken cancellationToken)
    {
        logger.LogDebug($"Worker received message: {inMessage}");

        var inSignal = ImportSignal.FromMessage(inMessage);

        var destination = UnzipFile(inSignal);

        await PublishSignal(destination, inSignal);
    }

    private string UnzipFile(ImportSignal inSignal)
    {
        var filename = Path.GetFileNameWithoutExtension(inSignal.Filepath);
        var destination = Path.Combine(_unzipPath, filename);

        file.Unzip(inSignal.Filepath, destination, _overwrite);
        return destination;
    }

    private async Task PublishSignal(string destination, ImportSignal inSignal)
    {
        var outSignal = ScanSignal.From(destination, inSignal);
        var message = outSignal.ToJson();

        await client.PublishAsync(message, _exchange);
    }
}
