using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Scanner;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Abstractions
{
    public interface IFileClassifier
    {
        int ClassifierVersion { get; }

        DataSources Source { get; }

        ImportClassification Classify(FileProbe probe);
    }
}
