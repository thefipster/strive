using TheFipster.ActivityAggregator.Api.Features.Upload.Mediators.Contracts;
using TheFipster.ActivityAggregator.Api.Features.Upload.Models;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;

namespace TheFipster.ActivityAggregator.Api.Features.Upload.Mediators.Decorators;

public class ZipsActionValidator(IZipsAction component) : IZipsAction
{
    public PagedResult<ZipIndex> GetZipFilePage(UploadFilePageRequest request)
    {
        if (!request.IsValid)
            throw new ArgumentException("Paging parameters are invalid");

        return component.GetZipFilePage(request);
    }
}
