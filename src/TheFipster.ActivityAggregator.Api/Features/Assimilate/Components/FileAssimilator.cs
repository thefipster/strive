using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Extensions;
using TheFipster.ActivityAggregator.Domain.Models.Importing;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Importer.Features.Extraction.Components.Contracts;
using TheFipster.ActivityAggregator.Storage.Abstractions.Features.Indexing.Components;

namespace TheFipster.ActivityAggregator.Api.Features.Assimilate.Components;

public class FileAssimilator(
    IIndexer<ExtractorIndex> extractInventory,
    IEnumerable<IFileExtractor> extractors,
    IExtractionCataloger cataloger,
    ILogger<FileAssimilator> logger
) : IFileAssimilator
{
    public async Task<ExtractorIndex?> ConvergeFileAsync(FileIndex file, CancellationToken ct)
    {
        var index = extractInventory.GetById(file.Hash);
        if (index != null)
            return index;

        var extractor = extractors.FirstOrDefault(x => x.Source == file.Source);
        if (extractor == null)
            return null;

        return await TryExtractAsync(file, ct, extractor);
    }

    private async Task<ExtractorIndex?> TryExtractAsync(
        FileIndex file,
        CancellationToken ct,
        IFileExtractor extractor
    )
    {
        try
        {
            return await ExtractAsync(file, extractor, ct);
        }
        catch (ExtractionException e)
        {
            LogExtractionError(file, e);
            return null;
        }
        catch (Exception e)
        {
            LogUnexpectedError(file, e);
            return null;
        }
    }

    private async Task<ExtractorIndex?> ExtractAsync(
        FileIndex file,
        IFileExtractor extractor,
        CancellationToken ct
    )
    {
        var extracts = new List<ExtractionMeta>();
        var metrics = new List<string>();
        long size = 0;
        var request = ExtractionRequest.New(file);
        var extractions = extractor.Extract(request);

        foreach (var extract in extractions)
        {
            metrics.AddRange(extract.Attributes.Keys.Select(x => x.ToString()));
            metrics.AddRange(extract.Series.Keys.Select(x => x.ToString()));
            metrics.AddRange(extract.Events.Select(x => x.Type.ToString()));

            var meta = await cataloger.HandleExtractAsync(file, extract, ct);

            extracts.Add(meta);
            size += meta.Size;
        }

        var valueHash = extractions.Select(x => x.ToHash()).ToUnorderedCollectionHash();

        return ExtractorIndex.New(file, valueHash, extracts, metrics, size);
    }

    private void LogUnexpectedError(FileIndex file, Exception e) =>
        logger.LogError(
            e,
            "Unexpected error while assimilating file {Filepath}. Skipping.",
            file.Path
        );

    private void LogExtractionError(FileIndex file, ExtractionException e) =>
        logger.LogWarning(
            e,
            "Extraction error while assimilating file {Filepath}. Skipping.",
            file.Path
        );
}
