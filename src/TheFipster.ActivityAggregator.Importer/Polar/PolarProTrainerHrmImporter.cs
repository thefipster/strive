using System.Globalization;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models.Extraction;
using TheFipster.ActivityAggregator.Domain.Models.Formats;
using TheFipster.ActivityAggregator.Domain.Models.Scanner;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Polar
{
    public class PolarProTrainerHrmImporter : IFileImporter
    {
        public DataSources Source => DataSources.PolarProTrainerHrm;
        public int ClassifierVersion => 1;
        public int ExtractorVersion => 1;

        public ImportClassification Classify(FileProbe probe)
        {
            var text = probe.Text;

            if (string.IsNullOrEmpty(text))
                throw new ClassificationException(
                    probe.Filepath,
                    Source,
                    "Couldn't find any text."
                );

            if (text.Length < 8 || text.Substring(0, 8) != "[Params]")
                throw new ClassificationException(
                    probe.Filepath,
                    Source,
                    "Couldn't find params section."
                );

            var lines = text.Split('\n');

            var dateLine = lines.First(x => x.Substring(0, 4) == "Date");
            var dateParts = dateLine.Split("=");

            var timeLine = lines.First(x => x.Substring(0, 9) == "StartTime");
            var timeParts = timeLine.Split("=");

            var datetime = GetDate(dateParts[1], timeParts[1]);

            return new ImportClassification
            {
                Filepath = probe.Filepath,
                Source = Source,
                Datetime = datetime,
                Datetype = DateRanges.Time,
            };
        }

        public List<FileExtraction> Extract(ArchiveIndex file)
        {
            var hrm = new HrmFile(file.Filepath);
            var meta = hrm.GetParams();
            var samples = hrm.GetSamples();

            if (samples == null || samples.Count == 0)
                throw new ExtractionException(file.Filepath, "HR Data is empty.");

            var dateValue = meta["Date"];
            var startTimeValue = meta["StartTime"];
            var date = GetDate(dateValue, startTimeValue);

            var durationValue = meta["Length"];
            int duration = GetStartTime(durationValue);

            var interval = int.Parse(meta["Interval"]);
            var smode = meta["SMode"];

            var slot = 0;

            var result = new FileExtraction(Source, file.Filepath, date, DateRanges.Time);

            var heartrateSeries = samples[slot].Select(x => x.ToString()).ToList();
            var timeSeries = GenerateTimeSeries(date, interval, heartrateSeries);

            result.Attributes.Add(Parameters.StartTime, date.ToString(DateHelper.SecondFormat));
            result.Attributes.Add(Parameters.Duration, duration.ToString());

            result.Series.Add(Parameters.Timestamp, timeSeries);
            result.Series.Add(Parameters.Heartrate, heartrateSeries);

            if (smode.Substring(0, 1) == "1")
            {
                slot++;
                var speedSeries = samples[slot]
                    .Select(x => Math.Round(x / 10.0, 1).ToString(CultureInfo.InvariantCulture))
                    .ToList();

                result.Series.Add(Parameters.Speed, speedSeries);
            }

            if (smode.Substring(1, 1) == "1")
            {
                slot++;
                var cadenceSeries = samples[slot].Select(x => x.ToString()).ToList();
                result.Series.Add(Parameters.Cadence, cadenceSeries);
            }

            if (smode.Substring(2, 1) == "1")
            {
                slot++;
                var altitudeSeries = samples[slot].Select(x => x.ToString()).ToList();
                result.Series.Add(Parameters.AltitudeDelta, altitudeSeries);
            }

            return [result];
        }

        private List<string> GenerateTimeSeries(
            DateTime date,
            int interval,
            List<string> speedSeries
        )
        {
            var timeSeries = new List<string>();
            for (int i = 0; i < speedSeries.Count; i++)
                timeSeries.Add(date.AddSeconds(i * interval).ToString(DateHelper.SecondFormat));
            return timeSeries;
        }

        private static int GetStartTime(string durationValue)
        {
            var lengthSplit = durationValue.Split(".");
            var lengthParts = lengthSplit[0].Split(":");

            var hours = int.Parse(lengthParts[0]);
            var minutes = int.Parse(lengthParts[1]);
            var seconds = int.Parse(lengthParts[2]);

            var duration = seconds + (minutes * 60) + (hours * 3600);
            return duration;
        }

        private static DateTime GetDate(string dateValue, string startTimeValue)
        {
            var year = int.Parse(dateValue.Substring(0, 4));
            var month = int.Parse(dateValue.Substring(4, 2));
            var day = int.Parse(dateValue.Substring(6, 2));

            var splitParts = startTimeValue.Split('.');
            var timeParts = splitParts[0].Split(":");

            var hour = int.Parse(timeParts[0]);
            var minute = int.Parse(timeParts[1]);
            var second = int.Parse(timeParts[2]);

            var date = new DateTime(year, month, day, hour, minute, second);
            return date;
        }
    }
}
