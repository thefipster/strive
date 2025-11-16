using Fip.Strive.Harvester.Application.Core.Indexing.Contracts;
using Fip.Strive.Harvester.Application.Core.PubSub.Contracts;
using Fip.Strive.Harvester.Domain.Defaults;
using Fip.Strive.Harvester.Domain.Indexes;
using Fip.Strive.Harvester.Domain.Signals;
using Fip.Strive.Ingestion.Application.Services.Contracts;
using Fip.Strive.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Harvester.Pipeline.Classify.Cli.Application;

public class Worker(
    IPubSubClient client,
    IClassificationService classifier,
    ISetHashIndex<FileIndex> fileIndexer,
    ISetNameIndex<SourceIndex> sourceIndexer,
    IOptions<Config> config,
    ILogger<Worker> logger
) : IProcessor
{
    private readonly DirectExchange _exchange = HarvestPipelineExchange.New(
        SignalTypes.TypedSignal
    );

    private readonly string _filePath = config.Value.Path;

    public async Task ProcessAsync(string inMessage, CancellationToken ct)
    {
        logger.LogDebug($"Worker received message: {inMessage}");

        var inSignal = FileSignal.From(inMessage);

        await PublishFile(inSignal);

        var result = classifier.Classify(inSignal.Filepath, ct);

        var source = await PublishClassification(result, inSignal);

        if (source.Source != DataSources.NoSource)
            await PublishSignal(source, inSignal);
    }

    private async Task PublishFile(FileSignal inSignal)
    {
        var index = FileIndex.From(inSignal);
        await fileIndexer.SetHashAsync(index);
    }

    private async Task<SourceIndex> PublishClassification(
        List<ClassificationResult> result,
        FileSignal inSignal
    )
    {
        var classifications = result
            .Where(x => x.Classification != null)
            .Select(x => x.Classification!)
            .ToArray();

        var classifyHash = classifier.GetHash();
        var source =
            classifications.Length == 1
                ? SourceIndex.From(classifyHash, inSignal, classifications[0])
                : SourceIndex.From(classifyHash, inSignal);

        if (classifications.Length > 1)
            LogMultipleClassifications(classifications, inSignal);

        await sourceIndexer.SetFileAsync(source);
        return source;
    }

    private async Task PublishSignal(SourceIndex source, FileSignal inSignal)
    {
        var outSignal = TypedSignal.From(source, inSignal);
        await client.PublishAsync(outSignal.ToJson(), _exchange);
    }

    private void LogMultipleClassifications(Classification[] classifications, FileSignal inSignal)
    {
        var sources = string.Join(", ", classifications.Select(x => x.Source.ToString()));

        logger.LogWarning(
            "Multiple classifications found for file {Filepath}. {Sources}.",
            inSignal.Filepath,
            sources
        );
    }
}
