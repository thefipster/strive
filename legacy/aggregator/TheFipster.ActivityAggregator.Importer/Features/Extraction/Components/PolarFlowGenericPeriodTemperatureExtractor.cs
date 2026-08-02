using System.Globalization;
using System.Text.Json;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Domain.Vendor.Polar.Flow;
using TheFipster.ActivityAggregator.Importer.Features.Extraction.Components.Contracts;

namespace TheFipster.ActivityAggregator.Importer.Features.Extraction.Components
{
    public class PolarFlowGenericPeriodTemperatureExtractor : IFileExtractor
    {
        public DataSources Source => DataSources.PolarFlowGenericPeriodTemperature;
        public int ExtractorVersion => 1;

        public List<FileExtraction> Extract(ExtractionRequest file)
        {
            var json = File.ReadAllText(file.Filepath);
            var tempData =
                JsonSerializer.Deserialize<PolarFlowGenericPeriodTemperature>(json)
                ?? throw new ExtractionException(
                    file.Filepath,
                    "Couldn't parse polar takeout generic period temperature file."
                );

            if (
                tempData.Data?.TemperatureMeasurementSamples == null
                || tempData.Data.TemperatureMeasurementSamples.Count == 0
            )
                throw new ExtractionException(
                    file.Filepath,
                    "Couldn't find any temperature samples."
                );

            var result = new FileExtraction(Source, file.Filepath, file.Date, DateRanges.Day);
            result.Series.Add(Parameters.Timestamp, []);
            result.Series.Add(Parameters.Temperature, []);

            foreach (var sample in tempData.Data.TemperatureMeasurementSamples)
            {
                if (string.IsNullOrEmpty(sample.RecordingTimeDeltaMilliseconds))
                    continue;

                var temperature = sample.TemperatureCelsius.ToString(CultureInfo.InvariantCulture);
                var deltaMs = long.Parse(sample.RecordingTimeDeltaMilliseconds);
                var timestamp = file
                    .Date.AddMilliseconds(deltaMs)
                    .ToString(DateHelper.MillisecondFormat);

                result.Series[Parameters.Timestamp].Add(timestamp);
                result.Series[Parameters.Temperature].Add(temperature);
            }

            return [result];
        }
    }
}
