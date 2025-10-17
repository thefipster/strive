using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Importer.Features.Extraction.Services.Contracts;

public interface IExtractor
{
    Task<List<FileExtraction>> ExtractFile(FileIndex file);
}
