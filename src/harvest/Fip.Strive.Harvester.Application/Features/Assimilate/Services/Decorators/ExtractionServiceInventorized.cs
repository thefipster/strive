using Fip.Strive.Harvester.Application.Features.Assimilate.Models;
using Fip.Strive.Harvester.Application.Features.Assimilate.Services.Contracts;
using Fip.Strive.Indexing.Domain;
using Fip.Strive.Indexing.Domain.Models;
using Fip.Strive.Indexing.Storage.Contracts;
using Fip.Strive.Ingestion.Domain.Models;

namespace Fip.Strive.Harvester.Application.Features.Assimilate.Services.Decorators;

public class ExtractionServiceInventorized(IExtractionService component, IInventory inventory)
    : IExtractionService
{
    public Task WriteExtraction(FileExtraction extraction, WorkItem work, CancellationToken ct)
    {
        component.WriteExtraction(extraction, work, ct);

        var dateEntry = new DateEntry
        {
            Kind = extraction.Kind,
            Timestamp = extraction.Timestamp.ToUniversalTime(),
        };

        inventory.UpsertAsync(dateEntry);

        return Task.CompletedTask;
    }
}
