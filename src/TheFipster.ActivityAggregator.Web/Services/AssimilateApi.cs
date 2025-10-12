using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;

namespace TheFipster.ActivityAggregator.Web.Services;

public class AssimilateApi() : BaseApi("https://localhost:7098/")
{
    private const string BasePath = "api/assimilate";

    public async Task ExecuteAssimilation() => await ExecuteAction(BasePath);

    public async Task<Dictionary<DataSources, int>> GetExtractors()
    {
        var query = $"{BasePath}/extractors";
        return await GetDictionaryAsync<DataSources, int>(query);
    }

    public async Task<PagedResult<ExtractorIndex>> GetExtractsPageAsync(
        PagedRequest pagedRequest,
        string? classified = null,
        string? parameter = null,
        string? date = null
    )
    {
        var query = $"{BasePath}/extracts";
        query += $"?page={pagedRequest.Page}";
        query += $"&size={pagedRequest.Size}";

        if (!string.IsNullOrWhiteSpace(classified))
            query += $"&classified={classified}";

        if (!string.IsNullOrWhiteSpace(parameter))
            query += $"&parameter={parameter}";

        if (!string.IsNullOrWhiteSpace(date))
            query += $"&date={date}";

        return await GetPagedAsync<ExtractorIndex>(query);
    }
}
