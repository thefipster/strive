using Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;
using Fip.Strive.Core.Domain.Exceptions;
using Fip.Strive.Core.Domain.Schemas.Index.Models;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Core.Ingestion.Services.Contracts;
using Fip.Strive.Harvester.Application.Core.Indexing.Contracts;
using Fip.Strive.Harvester.Application.Features.Assimilate.Models;
using Fip.Strive.Harvester.Application.Features.Assimilate.Services.Contracts;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Harvester.Application.Features.Assimilate.Services;

public class AssimilationService(
    IExtractor extractors,
    IFileHasher hasher,
    IIndexer<DataIndex, string> indexer,
    IIndexer<FileIndex, string> files,
    IInventory inventory,
    IOptions<AssimilateConfig> config
) : IAssimilationService
{
    private readonly string _rootPath = config.Value.Path;

    public async Task<WorkItem> ExtractFileAsync(TypedSignal signal, CancellationToken ct)
    {
        WorkItem work = WorkItem.FromSignal(signal);

        AppendExtractions(work);
        foreach (var extraction in work.Extractions)
            await WriteExtraction(extraction, work, ct);

        UpdateFileIndex(work);

        return work;
    }

    private void UpdateFileIndex(WorkItem work)
    {
        var index = files.Find(work.Signal.Hash);
        if (index == null)
            throw new ExtractionException(
                work.Signal.Filepath,
                $"No index found for hash {work.Signal.Hash}"
            );

        index.ExtractorVersion = work.Extractor!.ExtractorVersion;
        index.LastExtractionAt = DateTime.UtcNow;
        index.Extractions = work.Extractions.Count;
        index.ExtractionMinDate = work.Extractions.Min(x => x.Timestamp);
        index.ExtractionMaxDate = work.Extractions.Min(x => x.Timestamp);

        files.Upsert(index);
    }

    private void AppendExtractions(WorkItem work)
    {
        work.Extractor = extractors.Find(work.Signal.Source);
        if (work.Extractor == null)
            throw new ExtractionException(
                work.Signal.Filepath,
                $"No extractor found for source {work.Signal.Source}"
            );

        work.Extractions = work.Extractor.Extract(work.Signal.Filepath, work.Signal.Timestamp);
    }

    private async Task WriteExtraction(
        FileExtraction extraction,
        WorkItem work,
        CancellationToken ct = default
    )
    {
        var filepath = extraction.Write(_rootPath);

        await SaveIndexAsync(extraction, work, filepath, ct);
        SaveInventory(extraction);
    }

    private async Task SaveIndexAsync(
        FileExtraction extraction,
        WorkItem work,
        string filepath,
        CancellationToken ct = default
    )
    {
        var hash = await hasher.HashXx3Async(filepath, ct);
        work.Index = new DataIndex
        {
            Hash = hash,
            Filepath = filepath,
            Kind = extraction.Kind,
            Timestamp = extraction.Timestamp,
            Source = extraction.Source,
            SourceFile = extraction.SourceFile,
            ReferenceId = work.Signal.ReferenceId,
            SignalledAt = work.Signal.EmittedAt,
            ParentId = work.Signal.Hash,
            SignalId = work.Signal.Id,
        };

        indexer.Upsert(work.Index);
    }

    private void SaveInventory(FileExtraction extraction)
    {
        var dateEntry = new DateEntry { Kind = extraction.Kind, Timestamp = extraction.Timestamp };
        inventory.Upsert(dateEntry);
    }
}
