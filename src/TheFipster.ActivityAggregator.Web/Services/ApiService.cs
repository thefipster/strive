using System.Collections;
using System.Text.Json;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Standards;

namespace TheFipster.ActivityAggregator.Web.Services
{
    public class ApiService
    {
        private readonly HttpClient http;

        public ApiService()
        {
            http = new HttpClient();
            http.BaseAddress = new Uri("https://localhost:7098/", UriKind.Absolute);
        }

        public async Task<Dictionary<DateTime, int>> GetMonthIndexAsync(DateTime date)
        {
            var query = $"/Index/month?date={date:yyyy-MM-dd}";
            return await GetSingleAsync<Dictionary<DateTime, int>>(query);
        }

        public async Task<IEnumerable<MasterIndex>> GetDayIndexAsync(DateTime date)
        {
            var query = $"/Index/day?day={date:yyyy-MM-dd}";
            return await GetCollectionAsync<MasterIndex>(query);
        }

        public async Task<IEnumerable<MergedRecord>> GetDayActivityAsync(DateTime date)
        {
            var query = $"/Activity/day?day={date:yyyy-MM-dd}";
            return await GetCollectionAsync<MergedRecord>(query);
        }

        public async Task<IEnumerable<UnifyIndex>> GetConflictsAsync()
        {
            var query = "/Index/conflicts?page=0&size=20";
            var page = await GetPagedAsync<UnifyIndex>(query);
            return page.Items;
        }

        public async Task<IEnumerable<Inventory>> GetInvetoryAsync(int? year)
        {
            var query = $"/Inventory?year={year}";
            return await GetCollectionAsync<Inventory>(query);
        }

        private async Task<TResult> GetSingleAsync<TResult>(string query)
        {
            var json = await GetJsonBodyAsync(query);
            var item = JsonSerializer.Deserialize<TResult>(json, JsonStandards.Options);

            if (item == null)
                throw new InvalidDataException("Result couldn't be parsed.");

            return item;
        }

        private async Task<IEnumerable<TResult>> GetCollectionAsync<TResult>(string query)
        {
            var json = await GetJsonBodyAsync(query);
            var collection = JsonSerializer
                .Deserialize<IEnumerable<TResult>>(json, JsonStandards.Options)
                ?.ToList();

            if (collection == null)
                throw new InvalidDataException("Result couldn't be parsed.");

            return collection;
        }

        private async Task<PagedResult<TResult>> GetPagedAsync<TResult>(string query)
        {
            var json = await GetJsonBodyAsync(query);
            var page = JsonSerializer.Deserialize<PagedResult<TResult>>(
                json,
                JsonStandards.Options
            );

            if (page == null)
                throw new InvalidDataException("Result couldn't be parsed.");

            return page;
        }

        private async Task<string> GetJsonBodyAsync(string query)
        {
            var response = await http.GetAsync(query);
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"{response.StatusCode} - {response.ReasonPhrase}");

            return await response.Content.ReadAsStringAsync();
        }
    }
}
