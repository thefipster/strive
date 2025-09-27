using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Services.Abstractions;

public interface IScanner
{
    Task<ScannerIndex> CheckAsync(string filepath, CancellationToken ct);
}
