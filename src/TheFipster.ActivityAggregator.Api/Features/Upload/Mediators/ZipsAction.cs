using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Features.Upload.Mediators;

public class ZipsAction(IPagedIndexer<ZipIndex> zipIndex) : IZipsAction
{
    public PagedResult<ZipIndex> GetZipFilePage(ZipsPageRequest request)
    {
        var specifications = request.ToSpecification();
        return zipIndex.GetPaged(specifications);
    }
}
