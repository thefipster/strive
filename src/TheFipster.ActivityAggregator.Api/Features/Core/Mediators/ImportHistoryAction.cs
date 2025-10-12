using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Features.Core.Mediators;

public class ImportHistoryAction(IHistoryIndexer history) : IImportHistoryAction
{
    public HistoryIndex GetImportHistory(string date)
    {
        var result = history.GetProcessingPath(DateTime.Parse(date));
        if (result == null)
            throw new HttpResponseException(404, "Batch not found.");

        return result;
    }
}
