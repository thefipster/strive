using Fip.Strive.Core.Domain.Exceptions;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Components;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
using Fip.Strive.Core.Ingestion.Contracts;

namespace Fip.Strive.Core.Ingestion.Vendors.Generic.Gpx
{
    public class GenericGpxClassifier : IFileClassifier
    {
        public DataSources Source => DataSources.Gpx;
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

            if (!props.Contains("gpx"))
                throw new ClassificationException(probe.Filepath, Source, "Couldn't find gpx tag.");

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
