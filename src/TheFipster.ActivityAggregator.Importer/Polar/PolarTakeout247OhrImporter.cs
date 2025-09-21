using System.Text.Json;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;
using TheFipster.ActivityAggregator.Polar.Domain;

namespace TheFipster.ActivityAggregator.Importer.Polar
{
    public class PolarTakeout247OhrImporter : IFileImporter
    {
        public DataSources Source => DataSources.PolarTakeout247Ohr;

        private readonly HashSet<string> required =
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

            if (!required.IsSubsetOf(values.Keys))
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

        public List<FileExtraction> Extract(ArchiveIndex file)
        {
            var json = File.ReadAllText(file.Filepath);
            var ohrData =
                JsonSerializer.Deserialize<PolarTakeout247Ohr>(json)
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
