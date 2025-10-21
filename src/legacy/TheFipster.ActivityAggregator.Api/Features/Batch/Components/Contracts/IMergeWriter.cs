using TheFipster.ActivityAggregator.Domain.Models.Files;

namespace TheFipster.ActivityAggregator.Api.Features.Batch.Components.Contracts;

public interface IMergeWriter
{
    void Persist(List<MergedFile> results);
}
