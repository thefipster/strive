namespace TheFipster.ActivityAggregator.Api.Services.Contracts;

public interface IAssimilaterService
{
    Task ExtractFilesAsync(string destinationDirectory, CancellationToken ct);
}
