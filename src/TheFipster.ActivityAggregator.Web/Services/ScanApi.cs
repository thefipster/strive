using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;

namespace TheFipster.ActivityAggregator.Web.Services;

public class ScanApi() : BaseApi("https://localhost:7098/")
{
    private const string BasePath = "api/scan";

    public async Task ExecuteFileScan() => await ExecuteAction(BasePath);

    public async Task<PagedResult<FileIndex>> GetFilesAsync(
        PagedRequest pagedRequest,
        bool? classified = null,
        string? search = null
    )
    {
        var query = $"{BasePath}/files";
        query += $"?page={pagedRequest.Page}";
        query += $"&size={pagedRequest.Size}";

        if (classified.HasValue)
            query += $"&classified={classified.Value}";

        if (!string.IsNullOrWhiteSpace(search))
            query += $"&search={search}";

        return await GetPagedAsync<FileIndex>(query);
    }
}
