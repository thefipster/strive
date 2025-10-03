using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Extraction;

namespace TheFipster.ActivityAggregator.Importer.Abstractions
{
    public interface IFileExtractor
    {
        int ExtractorVersion { get; }

        DataSources Source { get; }

        List<FileExtraction> Extract(ArchiveIndex file);
    }
}
