using Fip.Strive.Ingestion.Application.Contracts;
using Fip.Strive.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Domain.Models;

namespace Fip.Strive.Ingestion.Application.Services.Contracts;

public interface IExtractor
{
    IEnumerable<IFileExtractor> GetAll();

    IFileExtractor? Find(DataSources source);

    Task<List<FileExtraction>> ExtractAsync(string filepath, DataSources source, DateTime? date);
}
