using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Importer.Features.Extraction.Components.Contracts;
using TheFipster.ActivityAggregator.Importer.Features.Extraction.Services.Contracts;

namespace TheFipster.ActivityAggregator.Importer.Features.Extraction.Services;

public class Extractor(IEnumerable<IFileExtractor> extractors) : IExtractor
{
    public Task<List<FileExtraction>> ExtractFile(FileIndex file)
    {
        var extractor = extractors.FirstOrDefault(x => x.Source == file.Source);
        if (extractor == null)
            throw new ExtractionException(
                file.Path,
                $"No extractor available for source {file.Source}."
            );

        var request = ExtractionRequest.New(file);
        var extractions = extractor.Extract(request);

        return Task.FromResult(extractions);
    }
}
