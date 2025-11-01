using Fip.Strive.Core.Domain.Exceptions;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
using Fip.Strive.Core.Ingestion.Contracts;
using Fip.Strive.Core.Ingestion.Services.Contracts;

namespace Fip.Strive.Core.Ingestion.Services;

public class Extractor(IEnumerable<IFileExtractor> extractors) : IExtractor
{
    public IFileExtractor? Find(DataSources source) =>
        extractors.FirstOrDefault(x => x.Source == source);

    public IEnumerable<IFileExtractor> GetAll() => extractors;

    public Task<List<FileExtraction>> ExtractAsync(
        string filepath,
        DataSources source,
        DateTime? date = null
    )
    {
        var extractor = extractors.FirstOrDefault(x => x.Source == source);
        if (extractor == null)
            throw new ExtractionException(filepath, $"No extractor available for source {source}.");

        var extractions = extractor.Extract(filepath, date);

        return Task.FromResult(extractions);
    }
}
