using System.Globalization;
using System.Text.Json;
using Fip.Strive.Core.Domain.Exceptions;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Components;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
using Fip.Strive.Core.Ingestion.Contracts;

namespace Fip.Strive.Core.Ingestion.Vendors.Polar.Flow.Ecg
{
    public class PolarFlowGenericPeriodEcgExtractor : IFileExtractor
    {
        public DataSources Source => DataSources.PolarFlowGenericPeriodEcg;
        public int ExtractorVersion => 1;

        public List<FileExtraction> Extract(string filepath, DateTime? date = null)
        {
            if (date == null)
                throw new ExtractionException(
                    filepath,
                    "Date is required for polar flow generic period ecg files."
                );

            var json = File.ReadAllText(filepath);
            var ecgTest =
                JsonSerializer.Deserialize<PolarFlowGenericPeriodEcg>(json)
                ?? throw new ExtractionException(
                    filepath,
                    "Couldn't parse polar takeout generic period ecg test file."
                );

            if (ecgTest.Data?.Samples == null || ecgTest.Data.Samples.Count == 0)
                throw new ExtractionException(filepath, "Couldn't find any ecg samples.");

            var result = new FileExtraction(Source, filepath, date.Value.Date, DataKind.Day);

            var testEvent = CreateTestEvent(ecgTest);
            result.Events.Add(testEvent);

            var series = CreateSeries(ecgTest, result.Timestamp);
            result.Series = series;

            return [result];
        }

        private Dictionary<Parameters, List<string>> CreateSeries(
            PolarFlowGenericPeriodEcg ecgTest,
            DateTime date
        )
        {
            var series = new Dictionary<Parameters, List<string>>();
            series.Add(Parameters.Timestamp, []);
            series.Add(Parameters.EcgAmplitudeMv, []);

            var qualitySeries = new List<string>();

            foreach (var sample in ecgTest.Data?.Samples ?? [])
            {
                var amplitude = sample.AmplitudeMv.ToString(CultureInfo.InvariantCulture);
                var timestamp = date.AddMilliseconds(sample.RecordingTimeDeltaMs)
                    .ToString(DateHelper.MillisecondFormat);

                series[Parameters.Timestamp].Add(timestamp);
                series[Parameters.EcgAmplitudeMv].Add(amplitude);

                var measurements = ecgTest.Data?.QualityMeasurements;
                if (measurements != null && measurements.Any())
                {
                    var quality = measurements.First(x =>
                        x.RecordingTimeDeltaMs == sample.RecordingTimeDeltaMs
                    );
                    if (!string.IsNullOrWhiteSpace(quality.QualityLevel))
                        qualitySeries.Add(quality.QualityLevel);
                }
            }

            if (qualitySeries.Count == series.First().Value.Count)
                series.Add(Parameters.EcgQuality, qualitySeries);

            return series;
        }

        private static UnifiedEvent CreateTestEvent(PolarFlowGenericPeriodEcg ecgTest)
        {
            if (ecgTest.Data?.TestTime == null)
                throw new ArgumentException("Ecg test has no start time.");

            var testTimeMs = long.Parse(ecgTest.Data.TestTime);
            var testTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(
                testTimeMs
            );

            var testEvent = new UnifiedEvent(EventTypes.EcgTest, testTime);
            testEvent.Values.Add(Parameters.Heartrate, ecgTest.Data.AverageHeartRateBpm.ToString());
            testEvent.Values.Add(
                Parameters.HeartrateVariability,
                ecgTest.Data.HeartRateVariabilityMs.ToString(CultureInfo.InvariantCulture)
            );

            return testEvent;
        }
    }
}
