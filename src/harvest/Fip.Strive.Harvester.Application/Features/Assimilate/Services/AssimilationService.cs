using Fip.Strive.Harvester.Application.Features.Assimilate.Models;
using Fip.Strive.Harvester.Application.Features.Assimilate.Services.Contracts;
using Fip.Strive.Harvester.Domain.Signals;
using Fip.Strive.Indexing.Application.Features.Contracts;
using Fip.Strive.Indexing.Domain;
using Fip.Strive.Ingestion.Application.Services.Contracts;
using Fip.Strive.Ingestion.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Fip.Strive.Harvester.Application.Features.Assimilate.Services;

public class AssimilationService(
    IExtractor extractors,
    IServiceScopeFactory scopeFactory,
    IIndexer<FileIndex, string> files
) : IAssimilationService
{
    public async Task<WorkItem> ExtractFileAsync(TypedSignal signal, CancellationToken ct)
    {
        WorkItem work = WorkItem.FromSignal(signal);

        AppendExtractions(work);
        foreach (var extraction in work.Extractions)
        {
            var scope = scopeFactory.CreateScope();
            var extractionService = scope.ServiceProvider.GetRequiredService<IExtractionService>();
            await extractionService.WriteExtraction(extraction, work, ct);
        }

        await UpdateFileIndexAsync(work);

        return work;
    }

    private async Task UpdateFileIndexAsync(WorkItem work)
    {
        var index = await files.FindAsync(work.Signal.Hash);
        if (index == null)
            throw new ExtractionException(
                work.Signal.Filepath,
                $"No index found for hash {work.Signal.Hash}"
            );

        index.ExtractorVersion = work.Extractor!.ExtractorVersion;
        index.LastExtractionAt = DateTime.UtcNow;
        index.Extractions = work.Extractions.Count;
        index.ExtractionMinDate = work.Extractions.Min(x => x.Timestamp).ToUniversalTime();
        index.ExtractionMaxDate = work.Extractions.Max(x => x.Timestamp).ToUniversalTime();

        await files.UpsertAsync(index);
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
}
