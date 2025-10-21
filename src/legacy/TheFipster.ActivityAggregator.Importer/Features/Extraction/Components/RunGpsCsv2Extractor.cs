using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Formats;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Features.Extraction.Components.Contracts;

namespace TheFipster.ActivityAggregator.Importer.Features.Extraction.Components
{
    public class RunGpsCsv2Extractor : IFileExtractor
    {
        public DataSources Source => DataSources.RunGpsCsvV2;
        public int ExtractorVersion => 1;

        public List<FileExtraction> Extract(ExtractionRequest file)
        {
            var csv = new CsvFile(file.Filepath, ";");
            var lines = csv.ReadLines().ToArray();
            var start = lines.First(x => x[0] == "TrainingStart");
            var end = lines.Last(x => x[0] == "TrainingStop");
            var positions = lines.Where(x => x[0] == "Position");

            var startDate = DateTime.Parse(start[9]);
            var endDate = DateTime.Parse(end[9]);

            var duration = (int)(endDate - startDate).TotalSeconds;

            var attributes = FileExtraction.EmptyAttributes;
            attributes.Add(Parameters.Duration, duration.ToString());
            attributes.Add(Parameters.StartTime, startDate.ToString(DateHelper.SecondFormat));
            attributes.Add(Parameters.Distance, end[8]);

            var timestampSeries = new List<string>();
            var latSeries = new List<string>();
            var lonSeries = new List<string>();
            var speedSeries = new List<string>();
            var headingSeries = new List<string>();
            var altitudeSeries = new List<string>();
            var distanceSeries = new List<string>();

            foreach (var position in positions)
            {
                latSeries.Add(position[1]);
                lonSeries.Add(position[2]);
                speedSeries.Add(position[3]);
                headingSeries.Add(position[4]);
                altitudeSeries.Add(position[5]);
                distanceSeries.Add(position[8]);

                var timestamp = DateTime.Parse(position[9]);
                timestampSeries.Add(timestamp.ToString(DateHelper.SecondFormat));
            }

            var series = FileExtraction.EmptySeries;
            series.Add(Parameters.Timestamp, timestampSeries);
            series.Add(Parameters.Latitude, latSeries);
            series.Add(Parameters.Longitude, lonSeries);
            series.Add(Parameters.Speed, speedSeries);
            series.Add(Parameters.Heading, headingSeries);
            series.Add(Parameters.Altitude, altitudeSeries);
            series.Add(Parameters.Distance, distanceSeries);

            var result = new FileExtraction(
                Source,
                file.Filepath,
                startDate,
                DateRanges.Time,
                attributes,
                series
            );
            return [result];
        }
    }
}
