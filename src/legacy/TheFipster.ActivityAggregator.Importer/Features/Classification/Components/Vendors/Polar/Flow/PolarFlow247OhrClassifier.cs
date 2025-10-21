using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models.Importing;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Contracts;

namespace TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Vendors.Polar.Flow
{
    public class PolarFlow247OhrClassifier : IFileClassifier
    {
        public DataSources Source => DataSources.PolarFlow247Ohr;
        public int ClassifierVersion => 1;

        private readonly HashSet<string> _required =
        [
            "deviceDays",
            "userId",
            "deviceId",
            "date",
            "samples",
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

            var date = values["date"];
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
                Datetype = DateRanges.Month,
            };
        }
    }
}
