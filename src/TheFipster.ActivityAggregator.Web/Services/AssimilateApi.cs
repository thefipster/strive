using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Web.Services;

public class AssimilateApi() : BaseApi("https://localhost:7098/")
{
    public async Task ExecuteAssimilation()
    {
        var query = "/api/assimilate";
        await ExecuteAction(query);
    }

    public async Task<PagedResult<ExtractorIndex>> GetFilesAsync(PagedRequest pagedRequest)
    {
        var query = $"api/assimilate/extracts?page={pagedRequest.Page}&size={pagedRequest.Size}";
        return await GetPagedAsync<ExtractorIndex>(query);
    }
}
