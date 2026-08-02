using TheFipster.ActivityAggregator.Domain.Models.Files;

namespace TheFipster.ActivityAggregator.Api.Features.Batch.Mediators.Contracts;

public interface IMergeFileAction
{
    MergedFile GetMergedFileByDate(string date);
}
