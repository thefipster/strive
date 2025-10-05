using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;

namespace TheFipster.ActivityAggregator.Web.Services;

public class HistoryApi() : BaseApi("https://localhost:7098/")
{
    private const string BasePath = "api/history";

    public async Task<HistoryIndex> GetImportHistory(DateTime timestamp)
    {
        var query = $"{BasePath}/import/{timestamp:yyyy-MM-ddTHH:mm:ss.fff}";
        return await GetSingleAsync<HistoryIndex>(query);
    }
}
