using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Features.Indexing.Components;

namespace TheFipster.ActivityAggregator.Api.Features.Batch.Mediators;

public class BatchExistsPerYearAction(IIndexer<BatchIndex> indexer) : IBatchExistsPerYearAction
{
    public IEnumerable<DateTime> GetExists(int year) =>
        indexer
            .GetFiltered(x => x.Timestamp.Year == year)
            .Select(x => x.Timestamp.Date)
            .Distinct()
            .OrderBy(x => x)
            .ToList();
}
