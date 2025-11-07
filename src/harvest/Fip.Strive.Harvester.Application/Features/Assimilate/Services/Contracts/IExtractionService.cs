using Fip.Strive.Harvester.Application.Features.Assimilate.Models;
using Fip.Strive.Ingestion.Domain.Models;

namespace Fip.Strive.Harvester.Application.Features.Assimilate.Services.Contracts;

public interface IExtractionService
{
    Task WriteExtraction(FileExtraction extraction, WorkItem work, CancellationToken ct);
}
