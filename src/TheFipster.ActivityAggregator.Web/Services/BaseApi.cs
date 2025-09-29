using System.Text.Json;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Standards;

namespace TheFipster.ActivityAggregator.Web.Services;

public abstract class BaseApi
{
    private readonly HttpClient http;

    public BaseApi(string baseUrl)
    {
        http = new HttpClient();
        http.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
    }

    protected async Task ExecuteAction(string query) => await GetBodyAsync(query);

    protected async Task<TResult> GetSingleAsync<TResult>(string query)
    {
        var json = await GetBodyAsync(query);
        var item = JsonSerializer.Deserialize<TResult>(json, JsonStandards.Options);

        if (item == null)
            throw new InvalidDataException("Result couldn't be parsed.");

        return item;
    }

    protected async Task<IEnumerable<TResult>> GetCollectionAsync<TResult>(string query)
    {
        var json = await GetBodyAsync(query);
        var collection = JsonSerializer
            .Deserialize<IEnumerable<TResult>>(json, JsonStandards.Options)
            ?.ToList();

        if (collection == null)
            throw new InvalidDataException("Result couldn't be parsed.");

        return collection;
    }

    protected async Task<PagedResult<TResult>> GetPagedAsync<TResult>(string query)
    {
        var json = await GetBodyAsync(query);
        var page = JsonSerializer.Deserialize<PagedResult<TResult>>(json, JsonStandards.Options);

        if (page == null)
            throw new InvalidDataException("Result couldn't be parsed.");

        return page;
    }

    protected async Task<string> GetBodyAsync(string query)
    {
        var response = await http.GetAsync(query);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"{(int)response.StatusCode} - {response.ReasonPhrase} - {body}"
            );
        }

        return await response.Content.ReadAsStringAsync();
    }
}
