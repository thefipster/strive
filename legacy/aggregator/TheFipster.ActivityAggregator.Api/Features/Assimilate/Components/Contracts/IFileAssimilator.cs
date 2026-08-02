using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Api.Features.Assimilate.Components.Contracts;

public interface IFileAssimilator
{
    Task<ExtractorIndex?> ConvergeFileAsync(FileIndex file, CancellationToken ct);
}
