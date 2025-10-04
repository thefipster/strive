using System.Globalization;
using System.Text.Json;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models.Extraction;
using TheFipster.ActivityAggregator.Domain.Models.Scanner;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Abstractions;
using TheFipster.ActivityAggregator.Polar.Domain;

namespace TheFipster.ActivityAggregator.Importer.Polar
{
    public class PolarTakeoutGenericPeriodTemperatureImporter : IFileImporter
    {
        private const string Type = "7";
        public DataSources Source => DataSources.PolarTakeoutGenericPeriodTemperature;
        public int ClassifierVersion => 1;
        public int ExtractorVersion => 1;

        private readonly HashSet<string> _required =
        [
            "meta",
            "id",
            "startTimeSeconds",
            "restOfBytes",
            "type",
            "temperatureMeasurementSamples",
        ];

        public ImportClassification Classify(FileProbe probe)
        {
            var values = probe.JsonValues;

            if (values == null)
                throw new ClassificationException(
                    probe.Filepath,
                    Source,
                    "Couldn't find valid json."
                );

            if (!_required.IsSubsetOf(values.Keys))
                throw new ClassificationException(
                    probe.Filepath,
                    Source,
                    "Couldn't find required properties."
                );

            var startTime = values["startTimeSeconds"];
            if (string.IsNullOrWhiteSpace(startTime))
                throw new ClassificationException(
                    probe.Filepath,
                    Source,
                    "Couldn't find date value."
                );

            if (values["type"] != Type)
                throw new ClassificationException(
                    probe.Filepath,
                    Source,
                    $"Couldn't match type {Type}"
                );

            var seconds = int.Parse(startTime);
            var date = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(seconds);

            return new ImportClassification
            {
                Filepath = probe.Filepath,
                Source = Source,
                Datetime = date,
                Datetype = DateRanges.Day,
            };
        }

        public List<FileExtraction> Extract(ArchiveIndex file)
        {
            var json = File.ReadAllText(file.Filepath);
            var tempData =
                JsonSerializer.Deserialize<PolarTakeoutGenericPeriodTemperature>(json)
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
