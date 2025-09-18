using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Abstractions
{
    public interface IFileClassifier
    {
        DataSources Source { get; }

        ImportClassification Classify(FileProbe probe);
    }
}
