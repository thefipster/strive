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

        public async Task<IEnumerable<ImporterIndex>> GetImporterIndexesAsync()
        {
            var query = "/api/index/importer/all";
            return await GetCollectionAsync<ImporterIndex>(query);
        }

        public async Task<IEnumerable<ScannerIndex>> GetScannerIndexesAsync(string hash)
        {
            var query = $"/api/index/scanner/all/{hash}";
            return await GetCollectionAsync<ScannerIndex>(query);
        }

        public async Task<int[]> GetScannerIndexCountAsync(string hash)
        {
            var query = $"/api/index/scanner/count/{hash}";
            return await GetSingleAsync<int[]>(query);
        }

        public async Task Scan(string hash)
        {
            var query = $"/api/processing/scan/{hash}";
            var response = await http.GetAsync(query);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"{(int)response.StatusCode} - {response.ReasonPhrase} - {body}"
                );
            }
        }

        public async Task Assimilate(string hash)
        {
            var query = $"/api/processing/assimilate/{hash}";
            var response = await http.GetAsync(query);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"{(int)response.StatusCode} - {response.ReasonPhrase} - {body}"
                );
            }
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
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"{(int)response.StatusCode} - {response.ReasonPhrase} - {body}"
                );
            }

            return await response.Content.ReadAsStringAsync();
        }
    }
}
