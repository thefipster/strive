using System.Text.Json;
using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Contracts;
using Fip.Strive.Ingestion.Domain.Components;
using Fip.Strive.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Domain.Models;

namespace Fip.Strive.Ingestion.Application.Vendors.Polar.Flow.Ohr
{
    public class PolarFlow247OhrExtractor : IFileExtractor
    {
        public DataSources Source => DataSources.PolarFlow247Ohr;
        public int ExtractorVersion => 1;

        public List<FileExtraction> Extract(string filepath, DateTime? _ = null)
        {
            var json = File.ReadAllText(filepath);
            var ohrData =
                JsonSerializer.Deserialize<PolarFlow247Ohr>(json)
                ?? throw new ArgumentException(
                    $"Couldn't parse polar takeout 247 ohr file {filepath}."
                );

            var results = new List<FileExtraction>();
            foreach (var day in ohrData.DeviceDays)
            {
                var date = day.Date;
                var result = new FileExtraction(Source, filepath, date, DataKind.Day);
                result.Series.Add(Parameters.Timestamp, []);
                result.Series.Add(Parameters.Heartrate, []);

                foreach (var samples in day.Samples)
                {
                    var timestamp = date.AddSeconds(samples.SecondsFromDayStart)
                        .ToString(DateHelper.SecondFormat);
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
