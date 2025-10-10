using TheFipster.ActivityAggregator.Api.Mediators.Upload.Contracts;
using TheFipster.ActivityAggregator.Api.Models.Requests;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Mediators.Upload;

public class ZipsAction(IPagedIndexer<ZipIndex> zipIndex) : IZipsAction
{
    public PagedResult<ZipIndex> GetZipFilePage(UploadFilePageRequest request)
    {
        var specifications = request.ToSpecification();
        return zipIndex.GetPaged(specifications);
    }
}
