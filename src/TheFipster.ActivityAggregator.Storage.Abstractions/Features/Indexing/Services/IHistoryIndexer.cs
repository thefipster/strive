using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Storage.Abstractions.Features.Indexing.Services;

public interface IHistoryIndexer
{
    HistoryIndex? GetProcessingPath(DateTime timestamp);
}
