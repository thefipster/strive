using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Features.Batch.Services;

public class BatchService(
    IInventoryIndexer inventory,
    IAssimilationGrouper grouper,
    IBackgroundTaskQueue queue
) : IBatchService
{
    public Task CombineFilesAsync(string convergancePath, CancellationToken ct)
    {
        var pageCount = int.MaxValue;
        var pageNo = 0;

        while (pageCount > 0 && !ct.IsCancellationRequested)
        {
            var page = inventory.GetDaysPaged(pageNo);
            pageNo++;
            pageCount = page.Items.Count();

            var days = page.Items.GroupBy(x => x.Timestamp.Date).Select(x => x.Key).ToList();
            days.ForEach(x => queue.Enqueue(async ctx => await grouper.CombinePerDayAsync(x, ctx)));
        }

        return Task.CompletedTask;
    }
}
