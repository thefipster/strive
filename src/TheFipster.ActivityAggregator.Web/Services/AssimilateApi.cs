using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;

namespace TheFipster.ActivityAggregator.Web.Services;

public class AssimilateApi() : BaseApi("https://localhost:7098/")
{
    private const string BasePath = "api/assimilate";

    public async Task ExecuteAssimilation() => await ExecuteAction(BasePath);

    public async Task<PagedResult<ExtractorIndex>> GetFilesAsync(PagedRequest pagedRequest)
    {
        var query = $"{BasePath}/extracts?page={pagedRequest.Page}&size={pagedRequest.Size}";
        return await GetPagedAsync<ExtractorIndex>(query);
    }

    public async Task<Dictionary<DataSources, int>> GetExtractors()
    {
        var query = $"{BasePath}/extractors";
        return await GetDictionaryAsync<DataSources, int>(query);
    }
}
