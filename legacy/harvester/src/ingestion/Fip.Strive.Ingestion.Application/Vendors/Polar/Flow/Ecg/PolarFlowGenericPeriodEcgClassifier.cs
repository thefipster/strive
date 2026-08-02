using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Contracts;
using Fip.Strive.Ingestion.Domain.Components;
using Fip.Strive.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Domain.Exceptions;
using Fip.Strive.Ingestion.Domain.Models;

namespace Fip.Strive.Ingestion.Application.Vendors.Polar.Flow.Ecg
{
    public class PolarFlowGenericPeriodEcgClassifier : IFileClassifier
    {
        private const string Type = "10";
        public DataSources Source => DataSources.PolarFlowGenericPeriodEcg;
        public int ClassifierVersion => 1;

        private readonly HashSet<string> _required =
        [
            "meta",
            "id",
            "startTimeSeconds",
            "restOfBytes",
            "type",
            "rriMs",
        ];

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

            var startTime = values["startTimeSeconds"];
            if (string.IsNullOrWhiteSpace(startTime))
                throw new ClassificationException(
                    probe.Filepath,
                    Source,
                    "Couldn't find date value."
                );

            if (values["type"] != Type)
                throw new ClassificationException(
                    probe.Filepath,
                    Source,
                    $"Couldn't match type {Type}"
                );

            var seconds = int.Parse(startTime);
            var date = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(seconds);

            return new ImportClassification
            {
                Filepath = probe.Filepath,
                Source = Source,
                Datetime = date,
                Datetype = DateRanges.Day,
            };
        }
    }
}
