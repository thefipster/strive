using System.Globalization;
using System.Text.Json;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Models.Scanner;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;
using TheFipster.ActivityAggregator.Polar.Domain;

namespace TheFipster.ActivityAggregator.Importer.Polar
{
    public class PolarTakeoutGenericPeriodSpo2Importer : IFileImporter
    {
        private const string Type = "11";
        public DataSources Source => DataSources.PolarTakeoutGenericPeriodSpo2;
        public int ClassifierVersion => 1;
        public int ExtractorVersion => 1;

        private readonly HashSet<string> required =
        [
            "meta",
            "id",
            "startTimeSeconds",
            "restOfBytes",
            "type",
            "bloodOxygenPercent",
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

            if (!required.IsSubsetOf(values.Keys))
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
            var spo2Test =
                JsonSerializer.Deserialize<PolarTakeoutGenericPeriodSpo2>(json)
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

        private static UnifiedEvent CreateTestEvent(PolarTakeoutGenericPeriodSpo2 spo2Test)
        {
            var testTimeMs = long.Parse(spo2Test.Data.TestTime);
            var testTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(
                testTimeMs
            );
            var testEvent = new UnifiedEvent(
                EventTypes.Spo2Measurement,
                testTime,
                spo2Test.Data.Spo2Class
            );

            testEvent.Values.Add(
                Parameters.BloodOxygenPercent,
                spo2Test.Data.BloodOxygenPercent.ToString()
            );
            return testEvent;
        }
    }
}
