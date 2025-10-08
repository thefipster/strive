using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

public interface IHistoryIndexer
{
    HistoryIndex? GetProcessingPath(DateTime timestamp);
}
