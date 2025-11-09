using Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;
using Fip.Strive.Harvester.Application.Features.Assimilate.Services.Contracts;
using Fip.Strive.Harvester.Domain.Signals;
using Fip.Strive.Indexing.Application.Repositories.Contracts;
using Fip.Strive.Indexing.Domain.Models;
using Fip.Strive.Indexing.Storage.Contracts;
using Fip.Strive.Ingestion.Domain.Exceptions;
using Fip.Strive.Ingestion.Domain.Models;
using Microsoft.Extensions.Options;
using IExtractionService = Fip.Strive.Ingestion.Application.Services.Contracts.IExtractionService;

namespace Fip.Strive.Harvester.Application.Features.Assimilate.Services;

public class AssimilateService(
    IAssimilateRepository assimilations,
    IExtractionService extractionService,
    IOptions<AssimilateConfig> config,
    IIndexerV2<ExtractIndexV2, string> extractions,
    IFileHasher hasher,
    IInventory inventory
) : IAssimilateService
{
    private readonly string _rootPath = config.Value.Path;

    public async Task ExtractFileAsync(TypedSignal signal, CancellationToken ct)
    {
        var index = await assimilations.FindAsync(signal.Hash);
        if (index != null && ExtractionIsUpToDate(index))
            return;

        var extractor = extractionService.Find(signal.Source);
        if (extractor == null)
            throw new ExtractionException(
                signal.Filepath,
                $"No extractor found for source {signal.Source}"
            );

        var extracts = extractor.Extract(signal.Filepath);

        foreach (var extraction in extracts)
            await WriteExtractionAsync(extraction, signal, ct);

        index = new AssimilateIndexV2
        {
            Filepath = signal.Filepath,
            Hash = signal.Hash,
            Source = extractor.Source,
            Version = extractor.ExtractorVersion,
        };

        await assimilations.SetAsync(index);
    }

    private async Task WriteExtractionAsync(
        FileExtraction extraction,
        TypedSignal signal,
        CancellationToken ct
    )
    {
        var filepath = extraction.Write(_rootPath);
        var hash = await hasher.HashXx3Async(filepath, ct);

        var index = new ExtractIndexV2
        {
            Hash = hash,
            ParentHash = signal.Hash,
            Filepath = filepath,
            ParentFilepath = signal.Filepath,
            Timestamp = extraction.Timestamp.ToUniversalTime(),
            Kind = extraction.Kind,
            Source = extraction.Source,
        };

        await extractions.SetAsync(index);

        var dateEntry = new DateEntry
        {
            Kind = extraction.Kind,
            Timestamp = extraction.Timestamp.ToUniversalTime(),
        };

        await inventory.UpsertAsync(dateEntry);
    }

    private bool ExtractionIsUpToDate(AssimilateIndexV2 index)
    {
        var extractor = extractionService.Find(index.Source);
        if (extractor == null)
            throw new InvalidOperationException(
                $"No classifier found for source {index.Source} even though it is a known hash {index.Hash}."
            );

        if (extractor.ExtractorVersion <= index.Version)
            return true;

        return false;
    }
}
