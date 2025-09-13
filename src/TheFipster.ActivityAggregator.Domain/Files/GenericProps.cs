using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheFipster.ActivityAggregator.Domain
{
    public class GenericProps
    {
        public GenericProps()
        {
            Data = new Dictionary<Parameters, string>();
        }

        public DateTime Date { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DateRanges Range { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FileTypes Type { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DataSources Source { get; set; }

        public Dictionary<Parameters, string> Data { get; set; }

        public static GenericProps FromFile(string filepath)
        {
            if (!File.Exists(filepath))
                throw new FileNotFoundException($"File {filepath} doesn't exist.");

            var json = File.ReadAllText(filepath);
            var param = JsonSerializer.Deserialize<GenericProps>(json);

            if (param == null)
                throw new ArgumentException("Is not a valid param file.", nameof(filepath));

            return param;
        }
    }
}
