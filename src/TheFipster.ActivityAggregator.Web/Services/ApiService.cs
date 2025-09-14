using System.Collections;
using System.Text.Json;
using TheFipster.ActivityAggregator.Domain.Models;

namespace Fipster.TrackMe.Web.Services
{
    public class ApiService
    {
        HttpClient http;

        public ApiService()
        {
            this.http = new HttpClient();
            http.BaseAddress = new Uri("https://localhost:7098/", UriKind.Absolute);
        }

        public async Task<Dictionary<DateTime, int>> GetMonthASync(DateTime date)
        {
            var result = await http.GetAsync($"/Extract?date={date:yyyy-MM-dd}");
            if (!result.IsSuccessStatusCode)
                throw new HttpRequestException($"{result.StatusCode} - {result.ReasonPhrase}");

            var json = await result.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Dictionary<DateTime, int>>(json)
                ?? throw new InvalidDataException("Result couldn't be parsed.");
        }

        public async Task<IEnumerable<FileExtraction>> GetDayAsync(DateTime date)
        {
            var result = new List<FileExtraction>();

            var dayResult = await http.GetAsync($"/Extract/day?date={date:yyyy-MM-dd}");
            if (dayResult.IsSuccessStatusCode)
            {
                var json = await dayResult.Content.ReadAsStringAsync();
                var dayFiles = JsonSerializer.Deserialize<List<FileExtraction>>(json);
                if (dayFiles != null && dayFiles.Any())
                    result.AddRange(dayFiles);
            }

            var sportResult = await http.GetAsync($"/Extract/sport?date={date:yyyy-MM-dd}");
            if (sportResult.IsSuccessStatusCode)
            {
                var json = await sportResult.Content.ReadAsStringAsync();
                var sportFiles = JsonSerializer.Deserialize<List<FileExtraction>>(json);
                if (sportFiles != null && sportFiles.Any())
                    result.AddRange(sportFiles);
            }

            return result;
        }
    }
}
