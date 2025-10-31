using System.Text.Json;
using Fip.Strive.Core.Domain.Exceptions;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
using Fip.Strive.Core.Ingestion.Contracts;

namespace Fip.Strive.Core.Ingestion.Vendors.Polar.Flow.Spo2
{
    public class PolarFlowGenericPeriodSpo2Extractor : IFileExtractor
    {
        public DataSources Source => DataSources.PolarFlowGenericPeriodSpo2;
        public int ExtractorVersion => 1;

        public List<FileExtraction> Extract(string filepath, DateTime? date = null)
        {
            if (date == null)
                throw new ExtractionException(
                    filepath,
                    "Date is required for polar flow generic period SPO2 files."
                );

            var json = File.ReadAllText(filepath);
            var spo2Test =
                JsonSerializer.Deserialize<PolarFlowGenericPeriodSpo2>(json)
                ?? throw new ExtractionException(
                    filepath,
                    "Couldn't parse polar takeout generic period SPO2 test file."
                );

            if (spo2Test.Data == null)
                throw new ExtractionException(filepath, "Couldn't find any SPO2 results.");

            var result = new FileExtraction(Source, filepath, date.Value, DataKind.Day);
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
