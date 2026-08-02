using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Importing;
using TheFipster.ActivityAggregator.Domain.Tools;

namespace TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Contracts
{
    public interface IFileClassifier
    {
        int ClassifierVersion { get; }

        DataSources Source { get; }

        ImportClassification Classify(FileProbe probe);
    }
}
