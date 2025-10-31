using Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;
using Fip.Strive.Core.Domain.Schemas.Index.Models;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Core.Ingestion.Services.Contracts;
using Fip.Strive.Harvester.Application.Core.Indexing.Repositories.Contracts;
using Fip.Strive.Harvester.Application.Features.Assimilate.Models;
using Fip.Strive.Harvester.Application.Features.Assimilate.Services.Contracts;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Harvester.Application.Features.Assimilate.Services;

public class AssimilationService(
    IExtractor extractor,
    IFileHasher hasher,
    IIndexer<DataIndex, string> indexer,
    IIndexer<FileIndex, string> files,
    IInventory inventory,
    IOptions<AssimilateConfig> config
) : IAssimilationService
{
    private string _rootPath = config.Value.Path;

    public async Task<WorkItem> ExtractFileAsync(TypedSignal signal, CancellationToken ct)
    {
        WorkItem work = WorkItem.FromSignal(signal);

        work.Extractions = await extractor.ExtractAsync(
            work.Signal.Filepath,
            work.Signal.Source,
            work.Signal.Timestamp
        );
        foreach (var extraction in work.Extractions)
            await HandleExtraction(extraction, work, ct);

        var index = files.Find(signal.Hash);
        // TODO Handle index

        return work;
    }

    private async Task HandleExtraction(
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
            SignalId = work.Signal.Id,
            ExtractorVersion = extraction.Version,
        };

        indexer.Upsert(work.Index);
    }

    private void SaveInventory(FileExtraction extraction)
    {
        var dateEntry = new DateEntry { Kind = extraction.Kind, Timestamp = extraction.Timestamp };
        inventory.Upsert(dateEntry);
    }
}
