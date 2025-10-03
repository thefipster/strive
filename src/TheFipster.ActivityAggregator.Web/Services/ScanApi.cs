using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;

namespace TheFipster.ActivityAggregator.Web.Services;

public class ScanApi() : BaseApi("https://localhost:7098/")
{
    public async Task ExecuteFileScan()
    {
        var query = "/api/scan";
        await ExecuteAction(query);
    }

    public async Task<PagedResult<FileIndex>> GetFilesAsync(PagedRequest pagedRequest)
    {
        var query = $"api/scan/files?page={pagedRequest.Page}&size={pagedRequest.Size}";
        return await GetPagedAsync<FileIndex>(query);
    }
}
