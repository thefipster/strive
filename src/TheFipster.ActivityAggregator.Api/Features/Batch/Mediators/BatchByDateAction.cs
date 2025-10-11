using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Features.Batch.Mediators;

public class BatchByDateAction(IIndexer<BatchIndex> indexer) : IBatchByDateAction
{
    public IEnumerable<BatchIndex> GetBatchByDate(string date) =>
        indexer.GetFiltered(x => x.Timestamp.Date == DateTime.Parse(date)).ToArray();
}
