namespace TheFipster.ActivityAggregator.Api.Services.Contracts;

public interface IBatchService
{
    Task CombineFilesAsync(string convergancePath, CancellationToken ct);
}
