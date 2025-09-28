using System.Data;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;
using TheFipster.ActivityAggregator.Services.Abstractions;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Services.Worker;

public class Assimilater(IIndexer<AssimilaterIndex> indexer, IImporterRegistry registry)
    : IAssimilater
{
    public Task<AssimilaterIndex> StandardizeAsync(ScannerIndex index, CancellationToken ct)
    {
        if (index.Files.Count == 0)
            throw new ArgumentException($"Index {index.Hash} has no files.", nameof(index));

        if (index.Classification == null)
            throw new ArgumentException($"Index {index.Hash} is not classified.", nameof(index));

        var file = index.Files.First();
        var source = index.Classification.Source;
        var reader = registry.LoadExtractors().FirstOrDefault(x => x.Source == source);
        if (reader == null)
            throw new ExtractionException(file, $"Could not find extractor for source {source}");

        var archive = new ArchiveIndex
        {
            Source = source,
            Date = index.Classification.Datetime,
            Filepath = file,
            Range = index.Classification.Range,
            Md5Hash = index.Hash,
        };

        var extractions = reader.Extract(archive);

        var assimilation = new AssimilaterIndex
        {
            OriginHash = index.OriginHash,
            ValueHash = index.Hash,
        };

        throw new NotImplementedException();

        return Task.FromResult(assimilation);
    }
}
