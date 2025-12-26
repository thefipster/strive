using Fip.Strive.Ingestion.Application.Contracts;
using Fip.Strive.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Domain.Models;

namespace Fip.Strive.Ingestion.Application.Services.Contracts;

public interface IExtractionService
{
    IEnumerable<IFileExtractor> GetAll();

    IFileExtractor? Find(DataSources source);

    Task<ExtractionResponse> ExtractAsync(
        string filepath,
        DataSources source,
        DateTime? date = null
    );
}
