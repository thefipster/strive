using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Contracts;

namespace TheFipster.ActivityAggregator.Api.Features.Scan.Mediators;

public class ClassifiersAction(IEnumerable<IFileClassifier> classifiers) : IClassifiersAction
{
    public Dictionary<DataSources, int> GetClassifiers() =>
        classifiers.ToDictionary(x => x.Source, y => y.ClassifierVersion);
}
