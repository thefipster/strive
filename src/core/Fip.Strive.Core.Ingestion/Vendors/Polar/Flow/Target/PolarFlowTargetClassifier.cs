using Fip.Strive.Core.Domain.Exceptions;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Components;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
using Fip.Strive.Core.Ingestion.Contracts;

namespace Fip.Strive.Core.Ingestion.Vendors.Polar.Flow.Target
{
    public class PolarFlowTargetClassifier : IFileClassifier
    {
        public DataSources Source => DataSources.PolarFlowTarget;
        public int ClassifierVersion => 1;

        private readonly HashSet<string> _required = ["exportVersion", "done", "name", "startTime"];

        public ImportClassification Classify(FileProbe probe)
        {
            var values = probe.JsonValues;

            if (values == null)
                throw new ClassificationException(
                    probe.Filepath,
                    Source,
                    "Couldn't find valid json."
                );

            if (!_required.IsSubsetOf(values.Keys))
                throw new ClassificationException(
                    probe.Filepath,
                    Source,
                    "Couldn't find required properties."
                );

            var date = values["startTime"];
            if (string.IsNullOrWhiteSpace(date))
                throw new ClassificationException(
                    probe.Filepath,
                    Source,
                    "Couldn't find date value."
                );

            return new ImportClassification
            {
                Filepath = probe.Filepath,
                Source = Source,
                Datetime = DateTime.Parse(date),
                Datetype = DateRanges.Session,
            };
        }
    }
}
