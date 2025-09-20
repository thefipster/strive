using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

public interface IMasterIndexer
{
    IEnumerable<MasterIndex> GetByDay(DateTime date);
}
