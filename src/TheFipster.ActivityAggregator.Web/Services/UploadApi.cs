using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;

namespace TheFipster.ActivityAggregator.Web.Services;

public class UploadApi() : BaseApi("https://localhost:7098/")
{
    private const string BasePath = "api/upload";

    public async Task<PagedResult<ZipIndex>> GetZipPageAsync(
        PagedRequest pagedRequest,
        string search
    )
    {
        var query = $"{BasePath}/zips";
        query += $"?page={pagedRequest.Page}";
        query += $"&size={pagedRequest.Size}";

        if (!string.IsNullOrWhiteSpace(search))
            query += $"&search={search}";

        return await GetPagedAsync<ZipIndex>(query);
    }
}
