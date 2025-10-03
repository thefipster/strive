using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Web.Services;

public class UploadApi() : BaseApi("https://localhost:7098/")
{
    public async Task<IEnumerable<ZipIndex>> GetZipsAsync()
    {
        var query = "/api/upload/zips";
        return await GetCollectionAsync<ZipIndex>(query);
    }
}
