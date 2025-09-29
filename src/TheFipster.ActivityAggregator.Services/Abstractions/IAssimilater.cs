using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Services.Abstractions;

public interface IAssimilater
{
    Task<AssimilaterIndex> StandardizeAsync(ScannerIndex index, CancellationToken ct);
}
