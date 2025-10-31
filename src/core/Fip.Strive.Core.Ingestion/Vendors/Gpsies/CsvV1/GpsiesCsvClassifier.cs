using Fip.Strive.Core.Domain.Exceptions;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Components;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
using Fip.Strive.Core.Ingestion.Contracts;

namespace Fip.Strive.Core.Ingestion.Vendors.Gpsies.CsvV1
{
    public class GpsiesCsvClassifier : IFileClassifier
    {
        public DataSources Source => DataSources.GpsiesCsv;
        public int ClassifierVersion => 1;

        private readonly List<string> _header = ["Latitude,Longitude,Elevation"];

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

            var directory = new FileInfo(probe.Filepath).Directory;
            var date = DateHelper.GetDateFromMyCollectionDirectory(directory);

            return new ImportClassification
            {
                Filepath = probe.Filepath,
                Source = Source,
                Datetime = date,
                Datetype = DateRanges.Session,
            };
        }
    }
}
