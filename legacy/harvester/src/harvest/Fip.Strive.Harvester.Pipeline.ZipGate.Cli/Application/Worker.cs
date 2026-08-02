using System.Text.Json;
using Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;
using Fip.Strive.Harvester.Application.Core.Indexing.Contracts;
using Fip.Strive.Harvester.Application.Core.PubSub.Contracts;
using Fip.Strive.Harvester.Application.Core.Signals;
using Fip.Strive.Harvester.Application.Defaults;
using Fip.Strive.Harvester.Application.Infrastructure.Indexing.Data;
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
        var destination = GetImportPath(inSignal);

        if (!await indexer.HashExistsAsync(inSignal.Hash))
            await ImportFileAsync(destination, inSignal);

        file.Delete(inSignal.Filepath);

        await indexer.SetFileAsync(ZipIndex.FromSignal(inSignal, destination));
    }

    private string GetImportPath(UploadSignal inSignal)
    {
        var filename = Path.GetFileName(inSignal.Filepath);
        var destination = Path.Combine(_importPath, filename);
        return destination;
    }

    private async Task ImportFileAsync(string destination, UploadSignal inSignal)
    {
        file.Copy(inSignal.Filepath, destination, _overwrite);

        await PublishHash(destination, inSignal);
        await PublishSignal(destination, inSignal);
    }

    private async Task PublishHash(string destination, UploadSignal inSignal)
    {
        await indexer.SetHashAsync(new ZipIndex { Filepath = destination, Hash = inSignal.Hash });
    }

    private async Task PublishSignal(string destination, UploadSignal inSignal)
    {
        var outSignal = ImportSignal.From(destination, inSignal);
        var outMessage = JsonSerializer.Serialize(outSignal);

        await client.PublishAsync(outMessage, _exchange);
    }
}
