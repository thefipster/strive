using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;

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
}
