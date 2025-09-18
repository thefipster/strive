using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Abstractions;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Polar
{
    public class PolarTakeout247OhrImporter : IFileClassifier
    {
        public DataSources Source => DataSources.PolarTakeout247Ohr;

        private readonly HashSet<string> required =
        [
            "deviceDays",
            "userId",
            "deviceId",
            "date",
            "samples",
        ];

        public ImportClassification Classify(FileProbe probe)
        {
            var values = probe.GetJsonPropertiesWithValues();

            if (!required.IsSubsetOf(values.Keys))
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
