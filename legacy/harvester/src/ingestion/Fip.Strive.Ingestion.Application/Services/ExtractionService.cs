using Fip.Strive.Ingestion.Application.Contracts;
using Fip.Strive.Ingestion.Application.Services.Contracts;
using Fip.Strive.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Domain.Exceptions;
using Fip.Strive.Ingestion.Domain.Models;

namespace Fip.Strive.Ingestion.Application.Services;

public class ExtractionService(IEnumerable<IFileExtractor> extractors) : IExtractionService
{
    public IFileExtractor? Find(DataSources source) =>
        extractors.FirstOrDefault(x => x.Source == source);

    public IEnumerable<IFileExtractor> GetAll() => extractors;

    public Task<ExtractionResponse> ExtractAsync(
        string filepath,
        DataSources source,
        DateTime? date = null
    )
    {
        var extractor = extractors.FirstOrDefault(x => x.Source == source);
        if (extractor == null)
            throw new ExtractionException(filepath, $"No extractor available for source {source}.");

        var extractions = extractor.Extract(filepath, date);

        var response = new ExtractionResponse
        {
            Extractions = extractions,
            Version = extractor.ExtractorVersion,
        };

        return Task.FromResult(response);
    }
}
