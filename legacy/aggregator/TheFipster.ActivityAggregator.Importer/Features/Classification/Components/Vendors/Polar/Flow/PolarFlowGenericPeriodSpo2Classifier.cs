using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models.Importing;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Contracts;

namespace TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Vendors.Polar.Flow
{
    public class PolarFlowGenericPeriodSpo2Classifier : IFileClassifier
    {
        private const string Type = "11";
        public DataSources Source => DataSources.PolarFlowGenericPeriodSpo2;
        public int ClassifierVersion => 1;

        private readonly HashSet<string> _required =
        [
            "meta",
            "id",
            "startTimeSeconds",
            "restOfBytes",
            "type",
            "bloodOxygenPercent",
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
