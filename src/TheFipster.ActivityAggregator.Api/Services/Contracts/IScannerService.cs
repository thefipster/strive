namespace TheFipster.ActivityAggregator.Api.Services.Contracts;

public interface IScannerService
{
    Task CheckDirectoryAsync(string destinationDirectory, CancellationToken ct);
}
