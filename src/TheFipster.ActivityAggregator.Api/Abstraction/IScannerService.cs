using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Api.Abstraction;

public interface IScannerService
{
    Task CheckDirectoryAsync(string destinationDirectory, CancellationToken ct);
}
