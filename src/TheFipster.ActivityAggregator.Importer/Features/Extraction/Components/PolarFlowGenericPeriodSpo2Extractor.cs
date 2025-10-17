using System.Text.Json;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Importing;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Domain.Vendor.Polar.Flow;
using TheFipster.ActivityAggregator.Importer.Features.Extraction.Components.Contracts;

namespace TheFipster.ActivityAggregator.Importer.Features.Extraction.Components
{
    public class PolarFlowGenericPeriodSpo2Extractor : IFileExtractor
    {
        public DataSources Source => DataSources.PolarFlowGenericPeriodSpo2;
        public int ExtractorVersion => 1;

        public List<FileExtraction> Extract(ExtractionRequest file)
        {
            var json = File.ReadAllText(file.Filepath);
            var spo2Test =
                JsonSerializer.Deserialize<PolarFlowGenericPeriodSpo2>(json)
                ?? throw new ExtractionException(
                    file.Filepath,
                    "Couldn't parse polar takeout generic period SPO2 test file."
                );

            if (spo2Test.Data == null)
                throw new ExtractionException(file.Filepath, "Couldn't find any SPO2 results.");

            var result = new FileExtraction(Source, file.Filepath, file.Date, DateRanges.Day);
            var testEvent = CreateTestEvent(spo2Test);
            result.Events.Add(testEvent);
            return [result];
        }

        private static UnifiedEvent CreateTestEvent(PolarFlowGenericPeriodSpo2 spo2Test)
        {
            var testTimeMs = long.Parse(spo2Test.Data?.TestTime ?? string.Empty);
            var testTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(
                testTimeMs
            );

            var testEvent = new UnifiedEvent(
                EventTypes.Spo2Measurement,
                testTime,
                spo2Test.Data?.Spo2Class
            );

            if (spo2Test.Data != null)
            {
                testEvent.Values.Add(
                    Parameters.BloodOxygenPercent,
                    spo2Test.Data.BloodOxygenPercent.ToString()
                );
            }

            return testEvent;
        }
    }
}
