using System.Text.Json.Serialization;

namespace Fipster.TrackMe.Domain
{
    public class GenericSeries
    {
        public GenericSeries()
        {
            Data = new Dictionary<Parameters, IEnumerable<string>>();
        }

        public DateTime Date { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DateRanges Range { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FileTypes Type { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DataSources Source { get; set; }

        public IEnumerable<string> Parameters => Data.Keys.Select(x => x.ToString());

        public Dictionary<Parameters, IEnumerable<string>> Data { get; set; }

        public static GenericSeries CreateSeries(Parameters param, IEnumerable<string> series) =>
            new GenericSeries
            {
                Data = new Dictionary<Parameters, IEnumerable<string>> { { param, series } },
            };

        public static GenericSeries CreateSeries(
            Parameters firstParam,
            IEnumerable<string> firstSeries,
            Parameters secondParam,
            IEnumerable<string> secondSeries
        ) =>
            new GenericSeries
            {
                Data = new Dictionary<Parameters, IEnumerable<string>>
                {
                    { firstParam, firstSeries },
                    { secondParam, secondSeries },
                },
            };

        public static GenericSeries CreateSeries(
            Parameters firstParam,
            IEnumerable<string> firstSeries,
            Parameters secondParam,
            IEnumerable<string> secondSeries,
            Parameters thirdParam,
            IEnumerable<string> thirdSeries
        ) =>
            new GenericSeries
            {
                Data = new Dictionary<Parameters, IEnumerable<string>>
                {
                    { firstParam, firstSeries },
                    { secondParam, secondSeries },
                    { thirdParam, thirdSeries },
                },
            };

        public static GenericSeries CreateSeries(
            Parameters firstParam,
            IEnumerable<string> firstSeries,
            Parameters secondParam,
            IEnumerable<string> secondSeries,
            Parameters thirdParam,
            IEnumerable<string> thirdSeries,
            Parameters fourthParam,
            IEnumerable<string> fourthSeries
        ) =>
            new GenericSeries
            {
                Data = new Dictionary<Parameters, IEnumerable<string>>
                {
                    { firstParam, firstSeries },
                    { secondParam, secondSeries },
                    { thirdParam, thirdSeries },
                    { fourthParam, fourthSeries },
                },
            };
    }
}
