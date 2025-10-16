using TheFipster.ActivityAggregator.Domain.Models.Files;

namespace TheFipster.ActivityAggregator.Api.Features.Batch.Components;

public class MergeWriter : IMergeWriter
{
    public void Persist(List<MergedFile> results) { }
}
