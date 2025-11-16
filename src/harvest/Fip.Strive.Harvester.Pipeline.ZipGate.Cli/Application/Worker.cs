using System.Text.Json;
using Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;
using Fip.Strive.Harvester.Application.Core.Indexing.Contracts;
using Fip.Strive.Harvester.Application.Core.PubSub.Contracts;
using Fip.Strive.Harvester.Domain.Defaults;
using Fip.Strive.Harvester.Domain.Indexes;
using Fip.Strive.Harvester.Domain.Signals;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Harvester.Pipeline.ZipGate.Cli.Application;

public class Worker(
    IPubSubClient client,
    IFullIndexer<ZipIndex> indexer,
    IFileService file,
    IOptions<Config> config,
    ILogger<Worker> logger
) : IProcessor
{
    private readonly string _importPath = config.Value.Path;
    private readonly bool _overwrite = config.Value.Overwrite;

    private readonly DirectExchange _exchange = HarvestPipelineExchange.New(
        SignalTypes.ImportSignal
    );

    public async Task ProcessAsync(string inMessage, CancellationToken ct)
    {
        logger.LogDebug($"Worker received message: {inMessage}");

        var inSignal = UploadSignal.FromMessage(inMessage);

        if (!await indexer.HashExistsAsync(inSignal.Hash))
            await ImportFile(inSignal);

        file.Delete(inSignal.Filepath);

        await indexer.SetFileAsync(ZipIndex.FromSignal(inSignal));
    }

    private async Task ImportFile(UploadSignal inSignal)
    {
        var destination = CopyFile(inSignal);

        await PublishHash(inSignal);
        await PublishSignal(destination, inSignal);
    }

    private string CopyFile(UploadSignal inSignal)
    {
        var filename = Path.GetFileName(inSignal.Filepath);
        var destination = Path.Combine(_importPath, filename);

        file.Copy(inSignal.Filepath, destination, _overwrite);

        return destination;
    }

    private async Task PublishHash(UploadSignal inSignal)
    {
        await indexer.SetHashAsync(
            new ZipIndex { Filepath = inSignal.Filepath, Hash = inSignal.Hash }
        );
    }

    private async Task PublishSignal(string destination, UploadSignal inSignal)
    {
        var outSignal = ImportSignal.From(destination, inSignal);
        var outMessage = JsonSerializer.Serialize(outSignal);

        await client.PublishAsync(outMessage, _exchange);
    }
}
