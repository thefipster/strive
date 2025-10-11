using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Api.Features.Scan.Components.Contracts;

public interface IFileScanner
{
    Task<FileIndex> HandleFile(string filepath, string zipHash, CancellationToken ct);
}
