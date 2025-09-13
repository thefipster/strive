using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Models;

namespace TheFipster.ActivityAggregator.Importer.Modules.Abstractions
{
    public interface IFileExtractor
    {
        DataSources Source { get; }

        List<FileExtraction> Extract(ArchiveIndex file);
    }
}
