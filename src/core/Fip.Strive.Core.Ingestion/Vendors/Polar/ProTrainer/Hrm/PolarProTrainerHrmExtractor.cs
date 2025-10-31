using System.Globalization;
using Fip.Strive.Core.Domain.Exceptions;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Components;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
using Fip.Strive.Core.Ingestion.Contracts;
using Fip.Strive.Core.Ingestion.Models;

namespace Fip.Strive.Core.Ingestion.Vendors.Polar.ProTrainer.Hrm
{
    public class PolarProTrainerHrmExtractor : IFileExtractor
    {
        public DataSources Source => DataSources.PolarProTrainerHrm;
        public int ExtractorVersion => 1;

        public List<FileExtraction> Extract(string filepath, DateTime? date = null)
        {
            var hrm = new HrmFile(filepath);
            var meta = hrm.GetParams();
            var samples = hrm.GetSamples();

            if (samples == null || samples.Count == 0)
                throw new ExtractionException(filepath, "HR Data is empty.");

            var dateValue = meta["Date"];
            var startTimeValue = meta["StartTime"];
            var startDate = GetDate(dateValue, startTimeValue);

            var durationValue = meta["Length"];
            int duration = GetDuration(durationValue);

            var endDate = startDate.AddSeconds(duration);

            var interval = int.Parse(meta["Interval"]);
            var smode = meta["SMode"];

            var slot = 0;

            var result = new FileExtraction(Source, filepath, startDate, DataKind.Session);

            var heartrateSeries = samples[slot].Select(x => x.ToString()).ToList();
            var timeSeries = GenerateTimeSeries(startDate, interval, heartrateSeries);

            result.Attributes.Add(
                Parameters.StartTime,
                startDate.ToString(DateHelper.SecondFormat)
            );
            result.Attributes.Add(Parameters.EndTime, endDate.ToString(DateHelper.SecondFormat));
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

        private static int GetDuration(string durationValue)
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
