using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Features.Indexing.Components;
using TheFipster.ActivityAggregator.Storage.Abstractions.Features.Indexing.Services;

namespace TheFipster.ActivityAggregator.Api.Features.Batch.Components;

public class AssimilationGrouper(
    IPessimisticMerger merger,
    IIndexer<AssimilateIndex> indexer,
    IInventoryIndexer inventory
) : IAssimilationGrouper
{
    public Task<Dictionary<MergedFile, List<AssimilateIndex>>> CombinePerDayAsync(
        DateTime day,
        CancellationToken ct
    )
    {
        var assimilations = indexer.GetFiltered(x => x.Timestamp.Date == day.Date).ToList();

        var results = new Dictionary<MergedFile, List<AssimilateIndex>>();
        AppendSessions(results, day, assimilations, ct);
        AppendDay(results, day, assimilations, ct);

        return Task.FromResult(results);
    }

    private void AppendDay(
        Dictionary<MergedFile, List<AssimilateIndex>> results,
        DateTime day,
        List<AssimilateIndex> assimilations,
        CancellationToken ct
    )
    {
        ct.ThrowIfCancellationRequested();

        var assimilationsOfDay = assimilations.Where(x => x.Kind == DataKind.Day).ToList();
        var dayMerge = merger.CombineAssimilationGroup(
            day.Date,
            DataKind.Day,
            assimilationsOfDay,
            ct
        );

        results.Add(dayMerge, assimilationsOfDay);
    }

    private void AppendSessions(
        Dictionary<MergedFile, List<AssimilateIndex>> results,
        DateTime day,
        List<AssimilateIndex> assimilations,
        CancellationToken ct
    )
    {
        ct.ThrowIfCancellationRequested();

        var entries = inventory.GetByDate(day.Date);
        foreach (var entry in entries.Where(x => !x.IsDay))
            AppendSession(results, entry, assimilations, ct);
    }

    private void AppendSession(
        Dictionary<MergedFile, List<AssimilateIndex>> results,
        InventoryIndex entry,
        List<AssimilateIndex> assimilations,
        CancellationToken ct
    )
    {
        ct.ThrowIfCancellationRequested();

        var assimilationOfEntry = assimilations.Where(x => x.Timestamp == entry.Timestamp).ToList();

        var sessionMerge = merger.CombineAssimilationGroup(
            entry.Timestamp,
            DataKind.Session,
            assimilationOfEntry,
            ct
        );

        results.Add(sessionMerge, assimilationOfEntry);
    }
}
