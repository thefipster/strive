using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Api.Abstraction;

public interface IBatchService
{
    Task CombineFilesAsync(string convergancePath, CancellationToken ct);
}
