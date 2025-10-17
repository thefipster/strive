using System.Text.Json;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Importing;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Domain.Vendor.Polar.Flow;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Polar
{
    public class PolarTakeoutGenericPeriodSkinContactImporter : IFileClassifier, IFileExtractor
    {
        private const string Type = "8";
        public DataSources Source => DataSources.PolarFlowGenericPeriodSkinContact;
        public int ClassifierVersion => 1;
        public int ExtractorVersion => 1;

        private readonly HashSet<string> _required =
        [
            "meta",
            "id",
            "startTimeSeconds",
            "restOfBytes",
            "type",
            "skinContactChanges",
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

        public List<FileExtraction> Extract(ExtractionRequest file)
        {
            var json = File.ReadAllText(file.Filepath);
            var tempData =
                JsonSerializer.Deserialize<PolarFlowGenericPeriodSkinContact>(json)
                ?? throw new ExtractionException(
                    file.Filepath,
                    "Couldn't parse polar takeout generic period skin contact file."
                );

            if (
                tempData.Data?.SkinContactChanges == null
                || tempData.Data.SkinContactChanges.Count == 0
            )
                throw new ExtractionException(
                    file.Filepath,
                    "Couldn't find any skin contact changes."
                );

            var result = new FileExtraction(Source, file.Filepath, file.Date, DateRanges.Day);
            result.Series.Add(Parameters.Timestamp, []);
            result.Series.Add(Parameters.DeviceWorn, []);

            foreach (var sample in tempData.Data.SkinContactChanges)
            {
                var temperature = sample.SkinContact.GetValueOrDefault().ToString();
                var deltaMs = long.Parse(sample.RecordingTimeDeltaMilliseconds ?? "-1");
                var timestamp = file
                    .Date.AddMilliseconds(deltaMs)
                    .ToString(DateHelper.MillisecondFormat);

                result.Series[Parameters.Timestamp].Add(timestamp);
                result.Series[Parameters.DeviceWorn].Add(temperature);
            }

            return [result];
        }
    }
}
