using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Requests;

namespace TheFipster.ActivityAggregator.Importer.Abstractions
{
    public interface IFileExtractor
    {
        int ExtractorVersion { get; }

        DataSources Source { get; }

        List<FileExtraction> Extract(ExtractionRequest file);
    }
}
