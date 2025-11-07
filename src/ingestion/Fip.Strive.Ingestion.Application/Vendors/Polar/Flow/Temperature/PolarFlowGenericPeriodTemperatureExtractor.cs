using System.Globalization;
using System.Text.Json;
using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Contracts;
using Fip.Strive.Ingestion.Domain.Components;
using Fip.Strive.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Domain.Exceptions;
using Fip.Strive.Ingestion.Domain.Models;

namespace Fip.Strive.Ingestion.Application.Vendors.Polar.Flow.Temperature
{
    public class PolarFlowGenericPeriodTemperatureExtractor : IFileExtractor
    {
        public DataSources Source => DataSources.PolarFlowGenericPeriodTemperature;
        public int ExtractorVersion => 1;

        public List<FileExtraction> Extract(string filepath, DateTime? date = null)
        {
            if (date == null)
                throw new ExtractionException(
                    filepath,
                    "Date is required for polar flow generic period temperature files."
                );

            var json = File.ReadAllText(filepath);
            var tempData =
                JsonSerializer.Deserialize<PolarFlowGenericPeriodTemperature>(json)
                ?? throw new ExtractionException(
                    filepath,
                    "Couldn't parse polar takeout generic period temperature file."
                );

            if (
                tempData.Data?.TemperatureMeasurementSamples == null
                || tempData.Data.TemperatureMeasurementSamples.Count == 0
            )
                throw new ExtractionException(filepath, "Couldn't find any temperature samples.");

            var result = new FileExtraction(Source, filepath, date.Value, DataKind.Day);
            result.Series.Add(Parameters.Timestamp, []);
            result.Series.Add(Parameters.Temperature, []);

            foreach (var sample in tempData.Data.TemperatureMeasurementSamples)
            {
                if (string.IsNullOrEmpty(sample.RecordingTimeDeltaMilliseconds))
                    continue;

                var temperature = sample.TemperatureCelsius.ToString(CultureInfo.InvariantCulture);
                var deltaMs = long.Parse(sample.RecordingTimeDeltaMilliseconds);
                var timestamp = date
                    .Value.Date.AddMilliseconds(deltaMs)
                    .ToString(DateHelper.MillisecondFormat);

                result.Series[Parameters.Timestamp].Add(timestamp);
                result.Series[Parameters.Temperature].Add(temperature);
            }

            return [result];
        }
    }
}
