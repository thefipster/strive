using System.Text.Json;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Domain.Vendor.Polar.Flow;
using TheFipster.ActivityAggregator.Importer.Features.Extraction.Components.Contracts;

namespace TheFipster.ActivityAggregator.Importer.Features.Extraction.Components
{
    public class PolarFlow247OhrExtractor : IFileExtractor
    {
        public DataSources Source => DataSources.PolarFlow247Ohr;
        public int ExtractorVersion => 1;

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
