using Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;
using Fip.Strive.Harvester.Application.Core.PubSub.Contracts;
using Fip.Strive.Harvester.Domain.Defaults;
using Fip.Strive.Harvester.Domain.Indexes;
using Fip.Strive.Harvester.Domain.Signals;
using Fip.Strive.Harvester.Pipeline.Scanner.Cli.Services;
using Microsoft.Extensions.Logging;

namespace Fip.Strive.Harvester.Pipeline.Scanner.Cli.Application;

public class Worker(
    IPubSubClient client,
    IDirectoryService directory,
    IFileHasher hasher,
    IScanIndexer checker,
    ILogger<Worker> logger
) : IProcessor
{
    private readonly DirectExchange _exchange = HarvestPipelineExchange.New(SignalTypes.FileSignal);

    public async Task ProcessAsync(string inMessage, CancellationToken ct)
    {
        logger.LogDebug($"Worker received message: {inMessage}");

        var inSignal = ScanSignal.From(inMessage);

        var files = directory.EnumerateAllFiles(inSignal.UnzipPath);

        foreach (var file in files)
        {
            var hash = await hasher.HashXx3Async(file, ct);

            if (!await checker.HashExistsAsync(hash))
                await PublishSignal(file, hash, inSignal);

            await PublishFile(file, hash, inSignal);
        }
    }

    private async Task PublishSignal(string file, string hash, ScanSignal inSignal)
    {
        var outSignal = FileSignal.From(file, hash, inSignal);
        var message = outSignal.ToJson();
        await client.PublishAsync(message, _exchange);
    }

    private async Task PublishFile(string file, string hash, ScanSignal inSignal)
    {
        var instance = FileInstance.From(file, hash, inSignal);
        await checker.SetFileAsync(instance);
    }
}
