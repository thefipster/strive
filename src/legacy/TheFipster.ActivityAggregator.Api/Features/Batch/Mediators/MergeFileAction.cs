using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Storage.Abstractions.Features.Indexing.Components;

namespace TheFipster.ActivityAggregator.Api.Features.Batch.Mediators;

public class MergeFileAction(IIndexer<BatchIndex> indexer) : IMergeFileAction
{
    public MergedFile GetMergedFileByDate(string date)
    {
        var timestamp = DateTime.Parse(date);
        var batch = indexer.GetById(timestamp);
        if (batch == null)
            throw new HttpResponseException(404, "Batch not found.");

        return MergedFile.FromFile(batch.Filepath);
    }
}
