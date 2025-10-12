using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Features.Batch.Services;

public class BatchService(
    IInventoryIndexer dateInventory,
    IAssimilationGrouper assimilationGrouper,
    INotifier notifier
) : IBatchService
{
    public async Task CombineFilesAsync(string convergancePath, CancellationToken ct)
    {
        var pageCount = int.MaxValue;
        var pageNo = 0;
        var counter = 0;

        while (pageCount > 0 && !ct.IsCancellationRequested)
        {
            var page = dateInventory.GetDaysPaged(pageNo);
            pageNo++;
            pageCount = page.Items.Count();

            foreach (var item in page.Items)
            {
                counter++;
                var isDay = item.IsDay ? DataKind.Day : DataKind.Session;
                await assimilationGrouper.CombinePerDay(item, isDay, ct);
            }

            await ReportProgressAsync(counter, page.Total);
        }
    }

    private async Task ReportProgressAsync(int count, int total)
    {
        var progress = Math.Round((double)count / total * 100, 1);
        var message = $"Batch {count} of {total}";

        await notifier.ReportProgressAsync(Const.Hubs.Importer.Actions.Batch, message, progress);
    }
}
