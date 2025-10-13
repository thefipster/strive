using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Features.Batch.Components;

public class AssimilationGrouper(
    IPessimisticMerger merger,
    IIndexer<AssimilateIndex> indexer,
    IInventoryIndexer inventory
) : IAssimilationGrouper
{
    public async Task<BatchIndex> CombinePerDay(
        InventoryIndex item,
        DataKind kind,
        CancellationToken ct
    )
    {
        var assimilations = indexer
            .GetFiltered(x => x.Kind == kind && x.Timestamp == item.Timestamp)
            .ToList();

        return await merger.HandleAssimilationGroupAsync(item.Timestamp, kind, assimilations, ct);
    }

    public async Task<IEnumerable<BatchIndex>> CombinePerDay(DateTime day, CancellationToken ct)
    {
        var entries = inventory.GetByDate(day.Date);

        var assimilations = indexer.GetFiltered(x => x.Timestamp.Date == day.Date).ToList();

        var results = new List<BatchIndex>();

        var assimilationsOfDay = assimilations.Where(x => x.Kind == DataKind.Day).ToList();
        var dayIndex = await merger.HandleAssimilationGroupAsync(
            day.Date,
            DataKind.Day,
            assimilationsOfDay,
            ct
        );
        results.Add(dayIndex);

        foreach (var entry in entries.Where(x => !x.IsDay))
        {
            var assimilationOfEntry = assimilations
                .Where(x => x.Timestamp == entry.Timestamp)
                .ToList();

            var sessionIndex = await merger.HandleAssimilationGroupAsync(
                entry.Timestamp,
                DataKind.Day,
                assimilationOfEntry,
                ct
            );

            results.Add(sessionIndex);
        }

        return results;
    }
}
