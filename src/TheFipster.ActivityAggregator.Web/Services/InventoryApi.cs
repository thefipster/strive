using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Web.Services;

public class InventoryApi() : BaseApi("https://localhost:7098/")
{
    public async Task<Dictionary<int, int[]>> GetInventoryAsync()
    {
        var query = "/api/index/inventory/yearly";
        return await GetSingleAsync<Dictionary<int, int[]>>(query);
    }

    public async Task<IEnumerable<InventoryIndex>> GetInventoryByYearAsync(int year)
    {
        var query = $"/api/index/inventory/year/{year}";
        return await GetCollectionAsync<InventoryIndex>(query);
    }
}
