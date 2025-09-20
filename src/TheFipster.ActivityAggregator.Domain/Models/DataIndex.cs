using System.Text.Json.Serialization;
using TheFipster.ActivityAggregator.Domain.Enums;

namespace TheFipster.ActivityAggregator.Domain.Models
{
    public class DataIndex
    {
        public DataIndex()
        {
            Filepath = string.Empty;
            SourceFile = string.Empty;
            Md5Hash = string.Empty;
        }

        public string Filepath { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DataSources Source { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DateRanges Range { get; set; }

        public DateTime Date { get; set; }

        public string Md5Hash { get; set; }

        public FileTypes Type { get; set; }

        public string SourceFile { get; set; }
    }
}
