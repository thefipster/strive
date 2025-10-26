using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;

namespace Fip.Strive.Core.Ingestion.Contracts
{
    public interface IFileExtractor
    {
        int ExtractorVersion { get; }

        DataSources Source { get; }

        List<FileExtraction> Extract(string filepath, DateTime? date = null);
    }
}
