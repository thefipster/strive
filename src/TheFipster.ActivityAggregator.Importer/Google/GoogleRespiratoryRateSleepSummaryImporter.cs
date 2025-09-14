using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Modules.Google
{
    public class GoogleRespiratoryRateSleepSummaryImporter : IFileClassifier
    {
        public string Type => "google_respiratory_rate_sleep_summary";
        public DataSources Source => DataSources.FitbitTakeoutRespiratoryRateSleepSummary;

        private List<string> Header = new()
        {
            "timestamp,deep sleep stats - milli breaths per minute,deep sleep stats - standard deviation milli breaths per minute,deep sleep stats - signal to noise,light sleep stats - milli breaths per minute,light sleep stats - standard deviation milli breaths per minute,light sleep stats - signal to noise,rem sleep stats - milli breaths per minute,rem sleep stats - standard deviation milli breaths per minute,rem sleep stats - signal to noise,full sleep stats - milli breaths per minute,full sleep stats - standard deviation milli breaths per minute,full sleep stats - signal to noise",
        };

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
    }
}
