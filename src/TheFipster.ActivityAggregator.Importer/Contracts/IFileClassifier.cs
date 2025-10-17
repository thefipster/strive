using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Importing;
using TheFipster.ActivityAggregator.Domain.Tools;

namespace TheFipster.ActivityAggregator.Importer.Abstractions
{
    public interface IFileClassifier
    {
        int ClassifierVersion { get; }

        DataSources Source { get; }

        ImportClassification Classify(FileProbe probe);
    }
}
