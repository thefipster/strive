using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Features.Extraction.Components.Contracts;

namespace TheFipster.ActivityAggregator.Api.Features.Assimilate.Mediators;

public class ExtractorsAction(IEnumerable<IFileExtractor> extractors) : IExtractorsAction
{
    public Dictionary<DataSources, int> GetExtractors() =>
        extractors.ToDictionary(x => x.Source, y => y.ExtractorVersion);
}
