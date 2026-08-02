namespace TheFipster.ActivityAggregator.Api.Features.Assimilate.Services.Contracts;

public interface IAssimilaterService
{
    Task ExtractFiles(string destinationDirectory, CancellationToken ct);
}
