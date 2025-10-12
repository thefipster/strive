using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Features.Assimilate.Services;

public class AssimilaterService(
    IPagedIndexer<FileIndex> fileInventory,
    IFileAssimilator fileAssimilator,
    INotifier notifier
) : IAssimilaterService
{
    public async Task ExtractFilesAsync(string destinationDirectory, CancellationToken ct)
    {
        try { }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        var pageNo = 0;
        var counter = 0;
        var currentPageSize = int.MaxValue;

        while (!ct.IsCancellationRequested && currentPageSize > 0)
        {
            var page = fileInventory.GetPaged(pageNo, 10);
            currentPageSize = page.Items.Count();
            pageNo++;

            foreach (var file in page.Items.Where(x => x.Source is not null))
            {
                counter++;
                await fileAssimilator.ConvergeFileAsync(file, ct);

                if (ct.IsCancellationRequested)
                    throw new OperationCanceledException();
            }

            await ReportProgress(page, counter);
        }
    }

    private async Task ReportProgress(PagedResult<FileIndex> page, int counter)
    {
        var progress = Math.Round((double)counter / page.Total * 100, 1);
        var message = $"File {counter} of {page.Total}";

        await notifier.ReportProgressAsync(
            Const.Hubs.Importer.Actions.Assimilate,
            message,
            progress
        );
    }
}
