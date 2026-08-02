namespace TheFipster.ActivityAggregator.Api.Features.Batch.Services.Contracts;

public interface IBatchService
{
    Task CombineFilesAsync(string convergancePath, CancellationToken ct);
}
