using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Web.Services;

public class ScanService() : BaseApi("https://localhost:7098/")
{
    public async Task ExecuteFileScan()
    {
        var query = "/api/scan";
        await ExecuteAction(query);
    }
}
