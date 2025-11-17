using Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;
using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Harvester.Application.Core.Indexing.Contracts;
using Fip.Strive.Harvester.Application.Core.PubSub.Contracts;
using Fip.Strive.Harvester.Domain.Indexes;
using Fip.Strive.Harvester.Domain.Signals;
using Fip.Strive.Ingestion.Application.Services.Contracts;
using Fip.Strive.Ingestion.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Harvester.Pipeline.Assimilate.Cli.Application;

public class Worker(
    IExtractionService extractor,
    IFileHasher hasher,
    ISetNameIndex<DataIndex> dataWriter,
    ISetHashIndex<ExtractIndex> extractWriter,
    IOptions<Config> config,
    ILogger<Worker> logger
) : IProcessor
{
    private readonly string _extractPath = config.Value.Path;

    public async Task ProcessAsync(string inMessage, CancellationToken ct)
    {
        logger.LogDebug($"Worker received message: {inMessage}");

        try
        {
            await TryPerformAsync(inMessage, ct);
        }
        catch (ExtractionException)
        {
            // ignored for now
        }
    }

    private async Task TryPerformAsync(string inMessage, CancellationToken ct)
    {
        var inSignal = TypedSignal.From(inMessage);

        var response = await extractor.ExtractAsync(inSignal.Filepath, inSignal.Source);

        foreach (var extract in response.Extractions)
        {
            var filepath = extract.Write(_extractPath);
            var hash = await hasher.HashXx3Async(filepath, ct);
            var isDay = extract.Kind == DataKind.Day;
            var timestamp = extract.Timestamp.ToUniversalTime();

            var dataIndex = DataIndex.From(filepath, hash, isDay, timestamp, inSignal);
            await dataWriter.SetFileAsync(dataIndex);
        }

        var extractIndex = ExtractIndex.From(response.Version, inSignal);
        await extractWriter.SetHashAsync(extractIndex);
    }
}
