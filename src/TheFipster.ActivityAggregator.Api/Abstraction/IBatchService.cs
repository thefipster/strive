namespace TheFipster.ActivityAggregator.Api.Abstraction;

public interface IBatchService
{
    Task CombineFilesAsync(string convergancePath, CancellationToken ct);
}
