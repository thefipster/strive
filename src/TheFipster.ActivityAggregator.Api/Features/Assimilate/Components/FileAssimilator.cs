using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Extensions;
using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Importing;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Importer.Features.Extraction.Services.Contracts;
using TheFipster.ActivityAggregator.Storage.Abstractions.Features.Indexing.Components;

namespace TheFipster.ActivityAggregator.Api.Features.Assimilate.Components;

public class FileAssimilator(
    IIndexer<ExtractorIndex> extractInventory,
    IExtractor extractor,
    IExtractionCataloger cataloger,
    ILogger<FileAssimilator> logger
) : IFileAssimilator
{
    public async Task<ExtractorIndex?> ConvergeFileAsync(FileIndex file, CancellationToken ct)
    {
        var index = extractInventory.GetById(file.Hash);
        if (index != null)
            return index;

        return await TryExtractAsync(file, ct);
    }

    private async Task<ExtractorIndex?> TryExtractAsync(FileIndex file, CancellationToken ct)
    {
        try
        {
            return await ExtractAsync(file, ct);
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

    private async Task<ExtractorIndex?> ExtractAsync(FileIndex file, CancellationToken ct)
    {
        var extractions = await extractor.ExtractFile(file);
        var valueHash = extractions.Select(x => x.ToHash()).ToUnorderedCollectionHash();

        long size = 0;
        var parameters = new List<string>();
        var extracts = new List<ExtractionMeta>();

        foreach (var extract in extractions)
        {
            AppendAllParameters(parameters, extract);
            var meta = await cataloger.HandleExtractAsync(file, extract, ct);
            extracts.Add(meta);
            size += meta.Size;
        }

        return ExtractorIndex.New(file, valueHash, extracts, parameters, size);
    }

    private void AppendAllParameters(List<string> parameters, FileExtraction extract)
    {
        parameters.AddRange(extract.Attributes.Keys.Select(x => x.ToString()));
        parameters.AddRange(extract.Series.Keys.Select(x => x.ToString()));
        parameters.AddRange(extract.Events.Select(x => x.Type.ToString()));
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
