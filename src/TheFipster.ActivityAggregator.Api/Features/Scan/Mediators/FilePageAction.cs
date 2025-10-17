using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Features.Scan.Mediators;

public class FilePageAction(IPagedIndexer<FileIndex> indexer) : IFilePageAction
{
    public PagedResponse<FileIndex> GetFilePage(ScanFilePageRequest request)
    {
        var specifications = request.ToSpecification();
        return indexer.GetPaged(specifications);
    }
}
