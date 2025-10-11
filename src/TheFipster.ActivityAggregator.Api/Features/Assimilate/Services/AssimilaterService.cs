using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Features.Assimilate.Services;

public class AssimilaterService(
    IPagedIndexer<FileIndex> fileInventory,
    IFileAssimilator fileAssimilator
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
        var currentPageSize = int.MaxValue;

        while (!ct.IsCancellationRequested && currentPageSize > 0)
        {
            var page = fileInventory.GetPaged(pageNo, 100);
            currentPageSize = page.Items.Count();
            pageNo++;

            foreach (var file in page.Items.Where(x => x.Source is not null))
            {
                await fileAssimilator.ConvergeFileAsync(file, ct);

                if (ct.IsCancellationRequested)
                    throw new OperationCanceledException();
            }
        }
    }
}
