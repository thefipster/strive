using System.Text.Json.Serialization;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Domain.Models.Requests
{
    public class ExtractionRequest
    {
        public required string Filepath { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DataSources Source { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DateRanges Range { get; set; }

        public DateTime Date { get; set; }

        public static ExtractionRequest New(FileIndex file)
        {
            return new ExtractionRequest
            {
                Source = file.Source!.Value,
                Date = file.Timestamp!.Value,
                Filepath = file.Path,
                Range = file.Range!.Value,
            };
        }
    }
}
