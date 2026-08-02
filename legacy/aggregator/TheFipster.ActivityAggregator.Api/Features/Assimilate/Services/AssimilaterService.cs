using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Features.Indexing.Components;

namespace TheFipster.ActivityAggregator.Api.Features.Assimilate.Services;

public class AssimilaterService(
    IPagedIndexer<FileIndex> fileInventory,
    IFileAssimilator fileAssimilator,
    IBackgroundTaskQueue queue
) : IAssimilaterService
{
    public Task ExtractFiles(string destinationDirectory, CancellationToken ct)
    {
        try { }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        var pageNo = 0;
        var currentPageSize = int.MaxValue;

        while (!ct.IsCancellationRequested && currentPageSize > 0)
        {
            var page = fileInventory.GetPaged(pageNo, 10);
            currentPageSize = page.Items.Count();
            pageNo++;

            foreach (var file in page.Items.Where(x => x.Source is not null))
                queue.Enqueue(ctx => fileAssimilator.ConvergeFileAsync(file, ctx));
        }

        return Task.CompletedTask;
    }
}
