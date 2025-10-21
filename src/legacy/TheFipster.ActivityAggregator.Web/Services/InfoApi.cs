namespace TheFipster.ActivityAggregator.Web.Services;

public class InfoApi() : BaseApi("https://localhost:7098/")
{
    private const string BasePath = "api/info";

    public async Task<Dictionary<string, string>> GetConfig()
    {
        var query = $"{BasePath}/config";
        return await GetDictionaryAsync<string, string>(query);
    }
}
