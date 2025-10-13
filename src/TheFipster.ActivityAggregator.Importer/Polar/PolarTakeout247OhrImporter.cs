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
    public class PolarTakeout247OhrImporter : IFileClassifier, IFileExtractor
    {
        public DataSources Source => DataSources.PolarFlow247Ohr;
        public int ClassifierVersion => 1;
        public int ExtractorVersion => 1;

        private readonly HashSet<string> _required =
        [
            "deviceDays",
            "userId",
            "deviceId",
            "date",
            "samples",
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

            var date = values["date"];
            if (string.IsNullOrWhiteSpace(date))
                throw new ClassificationException(
                    probe.Filepath,
                    Source,
                    "Couldn't find date value."
                );

            return new ImportClassification
            {
                Filepath = probe.Filepath,
                Source = Source,
                Datetime = DateTime.Parse(date),
                Datetype = DateRanges.Month,
            };
        }

        public List<FileExtraction> Extract(ExtractionRequest file)
        {
            var json = File.ReadAllText(file.Filepath);
            var ohrData =
                JsonSerializer.Deserialize<PolarFlow247Ohr>(json)
                ?? throw new ArgumentException(
                    $"Couldn't parse polar takeout 247 ohr file {file.Filepath}."
                );

            var results = new List<FileExtraction>();
            foreach (var day in ohrData.DeviceDays)
            {
                var date = day.Date;
                var result = new FileExtraction(Source, file.Filepath, date, DateRanges.Day);
                result.Series.Add(Parameters.Timestamp, []);
                result.Series.Add(Parameters.Heartrate, []);

                foreach (var samples in day.Samples)
                {
                    var timestamp = date.AddSeconds(samples.SecondsFromDayStart).ToString("s");
                    var heartrate = samples.HeartRate.ToString();

                    result.Series[Parameters.Timestamp].Add(timestamp);
                    result.Series[Parameters.Heartrate].Add(heartrate);
                }

                results.Add(result);
            }

            return results;
        }
    }
}
