namespace TheFipster.ActivityAggregator.Api.Abstraction;

public interface IAssimilaterService
{
    Task ExtractFilesAsync(string destinationDirectory, CancellationToken ct);
}
