using System.Text.Json;
using Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;
using Fip.Strive.Harvester.Domain;
using Fip.Strive.Harvester.Domain.Defaults;
using Fip.Strive.Harvester.Domain.Indexes;
using Fip.Strive.Harvester.Domain.Signals;
using Fip.Strive.Harvester.Pipeline.ZipGate.Cli.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Harvester.Pipeline.ZipGate.Cli;

public class Worker(
    IPubSubClient client,
    IHashIndexer hashes,
    IPathIndexer paths,
    IFileService fileService,
    IOptions<HarvestConfig> config,
    ILogger<Worker> logger
) : IMessageProcessor
{
    private readonly string _importPath = config.Value.ImportPath;
    private readonly bool _overwrite = config.Value.ImportOverwrite;

    public async Task ProcessAsync(string inMessage, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Worker received message: {inMessage}");

        var inSignal =
            JsonSerializer.Deserialize<UploadSignal>(inMessage)
            ?? throw new InvalidOperationException("Invalid message");

        var hashIndex = await hashes.GetAsync(inSignal.Hash);

        if (hashIndex == null)
            await ImportFile(inSignal);

        fileService.Delete(inSignal.Filepath);

        await paths.UpsertAsync(
            new ZipIndexV2 { Filepath = inSignal.Filepath, Hash = inSignal.Hash }
        );
    }

    private async Task ImportFile(UploadSignal inSignal)
    {
        var destination = CopyFile(inSignal);

        await PublishHash(inSignal);
        await PublishSignal(destination, inSignal);
    }

    private async Task PublishSignal(string destination, UploadSignal inSignal)
    {
        var outSignal = ImportSignal.From(destination, inSignal);
        var outMessage = JsonSerializer.Serialize(outSignal);
        await client.PublishAsync(outMessage, new ImportExchange());
    }

    private async Task PublishHash(UploadSignal inSignal)
    {
        await hashes.UpsertAsync(
            new ZipIndexV2 { Filepath = inSignal.Filepath, Hash = inSignal.Hash }
        );
    }

    private string CopyFile(UploadSignal inSignal)
    {
        var filename = Path.GetFileName(inSignal.Filepath);
        var destination = Path.Combine(_importPath, filename);
        fileService.Copy(inSignal.Filepath, destination, _overwrite);
        return destination;
    }
}
