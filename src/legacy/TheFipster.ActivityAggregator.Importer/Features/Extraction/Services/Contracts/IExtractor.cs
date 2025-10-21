using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Importer.Features.Extraction.Components.Contracts;

namespace TheFipster.ActivityAggregator.Importer.Features.Extraction.Services.Contracts;

public interface IExtractor
{
    IEnumerable<IFileExtractor> Extractors { get; }

    Task<List<FileExtraction>> ExtractFile(FileIndex file);
}
