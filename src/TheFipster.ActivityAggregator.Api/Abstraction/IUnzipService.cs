using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Api.Abstraction;

public interface IUnzipService
{
    Task<ZipIndex> ExtractAsync(string zipFilepath, string outputDirectory, CancellationToken ct);
}
