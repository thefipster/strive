using System.Data;
using System.Data.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Configs;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Extensions;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;
using TheFipster.ActivityAggregator.Services.Abstractions;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Services.Worker;

public class Assimilater(
    IIndexer<AssimilaterIndex> indexer,
    IIndexer<ConvergeIndex> convergeIndexer,
    IImporterRegistry registry,
    IInventoryIndexer inventory,
    IOptions<ApiConfig> config,
    ILogger<Assimilater> logger
) : IAssimilater
{
    public AssimilaterIndex Standardize(ScannerIndex index)
    {
        if (index.Files.Count == 0)
            throw new ArgumentException($"Index {index.Hash} has no files.", nameof(index));

        if (index.Classification == null)
            throw new ArgumentException($"Index {index.Hash} is not classified.", nameof(index));

        var file = index.Files.First();
        var source = index.Classification.Source;

        var assimilation =
            indexer.GetById(index.Hash)
            ?? new AssimilaterIndex
            {
                OriginHash = index.OriginHash,
                FileHash = index.Hash,
                Source = source,
            };

        var reader = registry.LoadExtractors().FirstOrDefault(x => x.Source == source);
        if (reader == null)
        {
            assimilation.Actions.Log(AssimilaterActions.NoExtractor);
            return Updated(assimilation);
        }

        if (assimilation.ExtractorVersion >= reader.ExtractorVersion)
        {
            assimilation.Actions.Log(AssimilaterActions.NoOperation);
            return Updated(assimilation);
        }

        var archive = new ArchiveIndex
        {
            Source = source,
            Date = index.Classification.Datetime,
            Filepath = file,
            Range = index.Classification.Range,
            Md5Hash = index.Hash,
        };

        try
        {
            var extractions = reader.Extract(archive);
            var hash = extractions.Select(x => x.ToHash()).ToUnorderedCollectionHash();

            foreach (var extraction in extractions)
            {
                extraction.Write(config.Value.ConvergeDirectoryPath);

                var convergeIndex = new ConvergeIndex
                {
                    FileHash = assimilation.FileHash,
                    OriginHash = assimilation.OriginHash,
                    Source = source,
                    Hash = extraction.ToHashString(),
                    Kind = extraction.Range == DateRanges.Time ? DataKind.Session : DataKind.Day,
                    Timestamp = extraction.Timestamp,
                };
                convergeIndex.Actions.Log(ConvergeActions.Converged);

                var inventoryIndex = InventoryIndex.Parse(convergeIndex);
                inventory.EnsureIndex(inventoryIndex);

                convergeIndexer.Set(convergeIndex);
            }

            assimilation.ValueHash = hash;
            assimilation.ExtractorVersion = reader.ExtractorVersion;
            assimilation.Count = extractions.Count;
            assimilation.Dates = extractions.Select(x => x.Timestamp.Date).Distinct().ToList();
            assimilation.Actions.Log(AssimilaterActions.Extracted);
        }
        catch (Exception e)
        {
            logger.LogWarning(
                e,
                "Unexpected error during assimilation of file {File} with source {Source}.",
                archive.Filepath,
                archive.Source
            );
            assimilation.Actions.Log(AssimilaterActions.ExtractionFailed);
        }

        return Updated(assimilation);
    }

    private AssimilaterIndex Updated(AssimilaterIndex assimilation)
    {
        indexer.Set(assimilation);
        return assimilation;
    }
}
