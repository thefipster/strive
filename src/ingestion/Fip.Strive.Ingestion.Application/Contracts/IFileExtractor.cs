using Fip.Strive.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Domain.Models;

namespace Fip.Strive.Ingestion.Application.Contracts
{
    public interface IFileExtractor
    {
        int ExtractorVersion { get; }

        DataSources Source { get; }

        List<FileExtraction> Extract(string filepath, DateTime? date = null);
    }
}
