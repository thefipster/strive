using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;

namespace TheFipster.ActivityAggregator.Web.Services;

public class ScanApi() : BaseApi("https://localhost:7098/")
{
    private const string BasePath = "api/scan";

    public async Task ExecuteFileScan() => await ExecuteAction(BasePath);

    public async Task<Dictionary<DataSources, int>> GetClassifiers()
    {
        var query = $"{BasePath}/classifiers";
        return await GetDictionaryAsync<DataSources, int>(query);
    }

    public async Task<PagedResponse<FileIndex>> GetFilesPageAsync(
        PagedRequest pagedRequest,
        string? range = null,
        string? classified = null,
        string? search = null
    )
    {
        var query = $"{BasePath}/files";
        query += $"?page={pagedRequest.Page}";
        query += $"&size={pagedRequest.Size}";

        if (!string.IsNullOrWhiteSpace(classified))
            query += $"&classified={classified}";

        if (!string.IsNullOrWhiteSpace(range))
            query += $"&range={range}";

        if (!string.IsNullOrWhiteSpace(search))
            query += $"&search={search}";

        return await GetPagedAsync<FileIndex>(query);
    }
}
