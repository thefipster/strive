namespace TheFipster.ActivityAggregator.Api.Features.Assimilate.Services.Contracts;

public interface IAssimilaterService
{
    Task ExtractFilesAsync(string destinationDirectory, CancellationToken ct);
}
