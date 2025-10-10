using TheFipster.ActivityAggregator.Api.Mediators.Upload.Contracts;
using TheFipster.ActivityAggregator.Api.Models.Requests;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;

namespace TheFipster.ActivityAggregator.Api.Mediators.Upload.Decorators;

public class ZipsActionValidator(IZipsAction component) : IZipsAction
{
    public PagedResult<ZipIndex> GetZipFilePage(UploadFilePageRequest request)
    {
        if (!request.IsValid)
            throw new ArgumentException("Paging parameters are invalid");

        return component.GetZipFilePage(request);
    }
}
