using TheFipster.ActivityAggregator.Domain;

namespace TheFipster.ActivityAggregator.Importer.Modules.Abstractions
{
    public interface IFileClassifier
    {
        DataSources Source { get; }

        ImportClassification? Classify(string filepath);
    }
}
