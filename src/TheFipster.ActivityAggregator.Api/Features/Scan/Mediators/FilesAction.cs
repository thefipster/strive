using TheFipster.ActivityAggregator.Api.Features.Scan.Mediators.Contracts;
using TheFipster.ActivityAggregator.Api.Features.Scan.Models;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Features.Scan.Mediators;

public class FilesAction(IPagedIndexer<FileIndex> indexer) : IFilesAction
{
    public PagedResult<FileIndex> GetFilePage(ScanFilePageRequest request)
    {
        var specifications = request.ToSpecification();
        return indexer.GetPaged(specifications);
    }
}
