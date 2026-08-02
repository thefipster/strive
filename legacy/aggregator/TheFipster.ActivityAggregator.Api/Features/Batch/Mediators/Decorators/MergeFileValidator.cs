using TheFipster.ActivityAggregator.Domain.Models.Files;

namespace TheFipster.ActivityAggregator.Api.Features.Batch.Mediators.Decorators;

public class MergeFileValidator(IMergeFileAction component) : IMergeFileAction
{
    public MergedFile GetMergedFileByDate(string date)
    {
        if (!DateTime.TryParse(date, out DateTime _))
            throw new ArgumentException("Invalid date format.", nameof(date));

        return component.GetMergedFileByDate(date);
    }
}
