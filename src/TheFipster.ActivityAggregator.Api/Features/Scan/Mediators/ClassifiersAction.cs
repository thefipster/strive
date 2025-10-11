using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Api.Features.Scan.Mediators;

public class ClassifiersAction(IImporterRegistry registry) : IClassifiersAction
{
    public Dictionary<DataSources, int> GetClassifiers() =>
        registry.LoadClassifiers().ToDictionary(x => x.Source, y => y.ClassifierVersion);
}
