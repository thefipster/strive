using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Web.Services;

public class ScanService() : BaseApi("https://localhost:7098/")
{
    public async Task ExecuteFileScan()
    {
        var query = "/api/scan";
        await ExecuteAction(query);
    }

    public async Task<PagedResult<FileIndex>> GetFilesAsync(PagedRequest pagedRequest)
    {
        var query = $"api/index/files/paged?page={pagedRequest.Page}&size={pagedRequest.Size}";
        return await GetPagedAsync<FileIndex>(query);
    }
}
