using System.Text.Json.Serialization;
using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Domain.Models.Requests
{
    public class ExtractionRequest
    {
        public ExtractionRequest()
        {
            Filepath = string.Empty;
        }

        public string Filepath { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DataSources Source { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DateRanges Range { get; set; }

        public DateTime Date { get; set; }
    }
}
