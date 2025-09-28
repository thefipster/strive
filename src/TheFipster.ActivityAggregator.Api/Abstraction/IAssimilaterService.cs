using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Api.Abstraction;

public interface IAssimilaterService
{
    Task ConvergeImportAsync(ImporterIndex import, CancellationToken ct);
}
