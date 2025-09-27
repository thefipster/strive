using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models.Scanner;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Polar
{
    public class PolarProTrainerPddImporter : IFileClassifier
    {
        public DataSources Source => DataSources.PolarProTrainerPdd;
        public int ClassifierVersion => 1;
        public int ExtractorVersion => 1;

        public ImportClassification Classify(FileProbe probe)
        {
            var text = probe.Text;

            if (string.IsNullOrWhiteSpace(text))
                throw new ClassificationException(
                    probe.Filepath,
                    Source,
                    "Couldn't find any text."
                );

            if (text.Length < 9 || text.Substring(0, 9) != "[DayInfo]")
                throw new ClassificationException(
                    probe.Filepath,
                    Source,
                    "Couldn't find day info section."
                );

            var lines = text.Split('\n');
            var line = lines[2];

            var kvp = line.Split("\t");
            var date = kvp[0];

            var year = date.Substring(0, 4);
            var month = date.Substring(4, 2);
            var day = date.Substring(6, 2);

            var datetime = DateTime.Parse($"{year}-{month}-{day}");

            return new ImportClassification
            {
                Filepath = probe.Filepath,
                Source = Source,
                Datetime = datetime,
                Datetype = DateRanges.Day,
            };
        }
    }
}
