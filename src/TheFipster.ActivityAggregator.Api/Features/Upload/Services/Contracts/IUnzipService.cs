using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Api.Services.Contracts;

public interface IUnzipService
{
    Task<ZipIndex> ExtractAsync(string zipFilepath, string outputDirectory, CancellationToken ct);
}
