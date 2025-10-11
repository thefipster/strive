using TheFipster.ActivityAggregator.Domain.Extensions;
using TheFipster.ActivityAggregator.Domain.Models.Importing;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Importer.Abstractions;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Features.Assimilate.Components;

public class FileAssimilator(
    IIndexer<ExtractorIndex> extractInventory,
    IImporterRegistry registry,
    IExtractionCataloger cataloger
) : IFileAssimilator
{
    public async Task<ExtractorIndex?> ConvergeFileAsync(FileIndex file, CancellationToken ct)
    {
        var index = extractInventory.GetById(file.Hash);
        if (index != null)
            return index;

        var extractor = registry.LoadExtractors().FirstOrDefault(x => x.Source == file.Source);
        if (extractor == null)
            return null;

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

        var extractHashes = extractions.Select(x => x.ToHash());
        var valueHash = extractHashes.ToUnorderedCollectionHash();

        return ExtractorIndex.New(file, valueHash, extracts, metrics, size);
    }
}
