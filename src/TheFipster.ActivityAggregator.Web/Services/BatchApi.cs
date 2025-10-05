using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Merging;
using TheFipster.ActivityAggregator.Domain.Models.Requests;

namespace TheFipster.ActivityAggregator.Web.Services;

public class BatchApi() : BaseApi("https://localhost:7098/")
{
    public async Task ExecuteMerge()
    {
        var query = "/api/batch";
        await ExecuteAction(query);
    }

    public async Task<PagedResult<BatchIndex>> GetFilesAsync(PagedRequest pagedRequest)
    {
        var query = $"api/batch/merge?page={pagedRequest.Page}&size={pagedRequest.Size}";
        return await GetPagedAsync<BatchIndex>(query);
    }

    public async Task<IEnumerable<BatchIndex>> GetDayAsync(DateTime date)
    {
        var query = $"api/batch/merge/{date:yyyy-MM-dd}";
        return await GetCollectionAsync<BatchIndex>(query);
    }

    public async Task<MergedFile> GetFileAsync(DateTime date)
    {
        var query = $"api/batch/merge/{date:yyyy-MM-dd}/file";
        return await GetSingleAsync<MergedFile>(query);
    }
}
