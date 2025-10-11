using TheFipster.ActivityAggregator.Api.Features.Upload.Models;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;

namespace TheFipster.ActivityAggregator.Api.Features.Upload.Mediators.Contracts;

public interface IZipsAction
{
    PagedResult<ZipIndex> GetZipFilePage(UploadFilePageRequest request);
}
