using TheFipster.ActivityAggregator.Api.Mediators.Upload.Contracts;
using TheFipster.ActivityAggregator.Api.Models.Requests;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Storage.Abstractions.Indexer;

namespace TheFipster.ActivityAggregator.Api.Mediators.Upload;

public class ZipsAction(IPagedIndexer<ZipIndex> zipIndex, ILogger<ZipsAction> logger) : IZipsAction
{
    public PagedResult<ZipIndex> TryGetZipFilePage(UploadFilePageRequest request)
    {
        try
        {
            return GetZipPage(request);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unexpected error getting zips.");
            throw new HttpResponseException(500, "Error getting zips", e.Message);
        }
    }

    private PagedResult<ZipIndex> GetZipPage(UploadFilePageRequest request)
    {
        var specifications = new PageSpecificationRequest<ZipIndex>();
        specifications.Page = request.Page;
        specifications.Size = request.Size;
        specifications.Sort = s => s.IndexedAt;
        specifications.IsDescending = true;

        if (!string.IsNullOrWhiteSpace(request.Search))
            specifications.AddFilter(f => f.ZipPath.Contains(request.Search));

        return zipIndex.GetPaged(specifications);
    }
}
