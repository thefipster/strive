using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Api.Features.Assimilate.Mediators;

public class ExtractorsAction(IImporterRegistry registry) : IExtractorsAction
{
    public Dictionary<DataSources, int> GetExtractors()
    {
        var extractors = registry.LoadExtractors();
        return extractors.ToDictionary(x => x.Source, y => y.ExtractorVersion);
    }
}
