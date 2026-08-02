using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Contracts;
using Fip.Strive.Ingestion.Domain.Components;
using Fip.Strive.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Domain.Exceptions;
using Fip.Strive.Ingestion.Domain.Models;

namespace Fip.Strive.Ingestion.Application.Vendors.Generic.Kml
{
    public class GenericKmlClassifier : IFileClassifier
    {
        public DataSources Source => DataSources.Kml;
        public int ClassifierVersion => 1;

        public ImportClassification Classify(FileProbe probe)
        {
            var props = probe.XmlTags;

            if (props == null)
                throw new ClassificationException(
                    probe.Filepath,
                    Source,
                    "Couldn't find xml tags."
                );

            if (!props.Contains("kml"))
                throw new ClassificationException(probe.Filepath, Source, "Couldn't find kml tag.");

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
