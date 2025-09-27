using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Services.Abstractions;

public interface IExtractor
{
    Task<ImporterIndex> ReadAsync(string zipFilepath, string outputDirectory, CancellationToken ct);
}
