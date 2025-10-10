using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Api.Components.Contracts;

public interface IFileScanner
{
    Task<FileIndex> HandleFile(string filepath, string zipHash, CancellationToken ct);
}
