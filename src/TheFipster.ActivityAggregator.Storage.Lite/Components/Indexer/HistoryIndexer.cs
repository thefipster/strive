using LiteDB;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;
using TheFipster.ActivityAggregator.Storage.Lite.Context;

namespace TheFipster.ActivityAggregator.Storage.Lite.Components.Indexer;

public class HistoryIndexer(IndexerContext context) : IHistoryIndexer
{
    protected readonly ILiteCollection<ZipIndex> Zips = context.GetCollection<ZipIndex>();
    protected readonly ILiteCollection<FileIndex> Files = context.GetCollection<FileIndex>();
    protected readonly ILiteCollection<ExtractorIndex> Extractions =
        context.GetCollection<ExtractorIndex>();
    protected readonly ILiteCollection<AssimilateIndex> Assimilations =
        context.GetCollection<AssimilateIndex>();
    protected readonly ILiteCollection<BatchIndex> Batches = context.GetCollection<BatchIndex>();

    public HistoryIndex? GetProcessingPath(DateTime timestamp)
    {
        var batch = Batches.FindById(new BsonValue(timestamp));
        if (batch == null)
            return null;

        var assimilations = new List<AssimilateIndex>();
        foreach (var assimilation in batch.Assimilations)
        {
            var index = Assimilations.Find(x => x.Hash == assimilation.Key);
            if (index != null)
                assimilations.AddRange(index);
        }
        assimilations = assimilations.DistinctBy(x => x.Hash).ToList();

        var extractions = new List<ExtractorIndex>();
        foreach (var assimilation in assimilations)
        {
            var index = Extractions.FindById(assimilation.FileHash);
            if (index != null)
                extractions.Add(index);
        }
        extractions = extractions.DistinctBy(x => x.FileHash).ToList();

        var files = new List<FileIndex>();
        foreach (var extraction in extractions)
        {
            extraction.ExtractedFiles.Clear();

            var index = Files.FindById(extraction.FileHash);
            if (index != null)
                files.Add(index);
        }
        files = files.DistinctBy(x => x.Hash).ToList();

        var zips = new List<ZipIndex>();
        foreach (var file in files)
        {
            var index = Zips.FindById(file.ZipHash);
            if (index != null)
                zips.Add(index);
        }
        zips = zips.DistinctBy(x => x.Hash).ToList();

        return new HistoryIndex
        {
            Batch = batch,
            Assimilations = assimilations,
            Extractions = extractions,
            Files = files,
            Zips = zips,
        };
    }
}
