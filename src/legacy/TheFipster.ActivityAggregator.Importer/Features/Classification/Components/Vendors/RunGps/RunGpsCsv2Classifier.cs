using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models.Importing;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Contracts;

namespace TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Vendors.RunGps
{
    public class RunGpsCsv2Classifier : IFileClassifier
    {
        public DataSources Source => DataSources.RunGpsCsvV2;
        public int ClassifierVersion => 1;

        private readonly List<string> _header =
        [
            "type;latitude;longitude;speed;heading;altitude;slope;ascentRate;distance;timestamp;heartRate;gpsDilution;temperature;cadence;",
        ];

        public ImportClassification Classify(FileProbe probe)
        {
            var lines = probe.Lines?.Take(2).ToArray();

            if (lines == null)
                throw new ClassificationException(
                    probe.Filepath,
                    Source,
                    "Couldn't get any lines."
                );

            if (lines.Length != 2)
                throw new ClassificationException(
                    probe.Filepath,
                    Source,
                    "Couldn't get two lines."
                );

            if (_header.All(x => x != lines.First()))
                throw new ClassificationException(probe.Filepath, Source, "Couldn't match header.");

            var data = lines.Last().Split(";", StringSplitOptions.RemoveEmptyEntries);
            var date = DateTime.Parse(data[9]);

            return new ImportClassification
            {
                Filepath = probe.Filepath,
                Source = Source,
                Datetime = date,
                Datetype = DateRanges.Time,
            };
        }
    }
}
