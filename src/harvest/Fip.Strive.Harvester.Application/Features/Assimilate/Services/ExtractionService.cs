using Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;
using Fip.Strive.Harvester.Application.Features.Assimilate.Models;
using Fip.Strive.Harvester.Application.Features.Assimilate.Services.Contracts;
using Fip.Strive.Indexing.Application.Features.Contracts;
using Fip.Strive.Indexing.Domain;
using Fip.Strive.Ingestion.Domain.Models;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Harvester.Application.Features.Assimilate.Services;

public class ExtractionService(
    IIndexer<DataIndex, string> indexer,
    IFileHasher hasher,
    IOptions<AssimilateConfig> config
) : IExtractionService
{
    private readonly string _rootPath = config.Value.Path;

    public async Task WriteExtraction(
        FileExtraction extraction,
        WorkItem work,
        CancellationToken ct = default
    )
    {
        var filepath = extraction.Write(_rootPath);
        await SaveIndexAsync(extraction, work, filepath, ct);
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
            Timestamp = extraction.Timestamp.ToUniversalTime(),
            Source = extraction.Source,
            SourceFile = extraction.SourceFile,
            ReferenceId = work.Signal.ReferenceId,
            SignalledAt = work.Signal.EmittedAt,
            ParentId = work.Signal.Hash,
            SignalId = work.Signal.Id,
        };

        await indexer.UpsertAsync(work.Index);
    }
}
