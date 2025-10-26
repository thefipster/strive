using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
using Fip.Strive.Core.Ingestion.Contracts;

namespace Fip.Strive.Core.Ingestion.Services.Contracts;

public interface IExtractor
{
    IEnumerable<IFileExtractor> Extractors { get; }

    Task<List<FileExtraction>> ExtractFile(
        string filepath,
        DataSources source,
        DateTime? date = null
    );
}
