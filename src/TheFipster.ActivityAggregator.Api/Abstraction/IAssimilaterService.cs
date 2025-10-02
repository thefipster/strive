using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Api.Abstraction;

public interface IAssimilaterService
{
    Task ExtractFilesAsync(string destinationDirectory, CancellationToken ct);

    Task ConvergeImportAsync(ImporterIndex import, CancellationToken ct);
}
