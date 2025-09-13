using Fipster.TrackMe.Domain;
using Fipster.TrackMe.Domain.Formats;
using Fipster.TrackMe.Domain.Models;
using Fipster.TrackMe.Domain.Tools;
using Fipster.TrackMe.Importer.Modules.Abstractions;

namespace Fipster.TrackMe.Importer.Modules.Google
{
    public class GoogleStepsImporter : IFileImporter
    {
        public string Type => "google_steps";
        public DataSources Source => DataSources.FitbitTakeoutSteps;

        private List<string> Header = ["timestamp,steps"];

        public ImportClassification? Classify(string filepath)
        {
            var peeker = new FilePeeker(filepath);

            var header = peeker.ReadLines(2);
            if (header.Count() != 2 || Header.All(x => x != header.First()))
                return null;

            var cells = header.Last().Split(",");
            var date = DateTime.Parse(cells[0]);

            return new ImportClassification
            {
                Filepath = filepath,
                Source = Source,
                Filetype = Type,
                Datetime = date,
                Datetype = DateRanges.Month,
            };
        }

        public List<FileExtraction> Extract(ArchiveIndex file)
        {
            var csv = new CsvFile(file.Filepath, ",");
            var data = csv.ReadLines().Skip(1).Select(x => (DateTime.Parse(x[0]), int.Parse(x[1])));
            var days = data.GroupBy(x => x.Item1.Date);
            var results = new List<FileExtraction>();

            foreach (var day in days)
            {
                var date = day.Key;
                var durationSeries = new List<string>();
                var stepsSeries = new List<string>();
                var steps = 0;

                foreach (var sample in day)
                {
                    var duration = (int)sample.Item1.TimeOfDay.TotalSeconds;
                    durationSeries.Add(duration.ToString());
                    stepsSeries.Add(sample.Item2.ToString());
                    steps += sample.Item2;
                }

                var series = new Dictionary<Parameters, IEnumerable<string>>()
                {
                    { Parameters.Duration, durationSeries },
                    { Parameters.Steps, stepsSeries },
                };

                var attributes = new Dictionary<Parameters, string>()
                {
                    { Parameters.Steps, steps.ToString() },
                };

                var result = new FileExtraction(
                    Source,
                    file.Filepath,
                    day.Key,
                    DateRanges.Day,
                    attributes,
                    series
                );
                results.Add(result);
            }

            return results;
        }
    }
}
