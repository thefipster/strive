using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Abstractions;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Gpsies
{
    public class GpsiesCsvImporter : IFileClassifier
    {
        public DataSources Source => DataSources.GpsiesCsv;

        private readonly List<string> header = ["Latitude,Longitude,Elevation"];

        public ImportClassification Classify(FileProbe probe)
        {
            var lines = probe.GetLines().Take(2).ToArray();

            if (lines.Length != 2)
                throw new ClassificationException(
                    probe.Filepath,
                    Source,
                    "Couldn't get two lines."
                );

            if (header.All(x => x != lines.First()))
                throw new ClassificationException(probe.Filepath, Source, "Couldn't match header.");

            return new ImportClassification
            {
                Filepath = probe.Filepath,
                Source = Source,
                Datetime = DateTime.MaxValue,
                Datetype = DateRanges.Day,
            };
        }
    }
}
