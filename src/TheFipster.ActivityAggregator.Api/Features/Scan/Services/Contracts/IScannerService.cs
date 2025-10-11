namespace TheFipster.ActivityAggregator.Api.Features.Scan.Services.Contracts;

public interface IScannerService
{
    Task CheckDirectoryAsync(string destinationDirectory, CancellationToken ct);
}
