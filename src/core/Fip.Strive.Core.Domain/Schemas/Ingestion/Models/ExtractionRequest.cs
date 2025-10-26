using System.Text.Json.Serialization;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;

namespace Fip.Strive.Core.Domain.Schemas.Ingestion.Models
{
    public class ExtractionRequest
    {
        public required string Filepath { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DataSources Source { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DateRanges Range { get; set; }

        public DateTime Date { get; set; }
    }
}
