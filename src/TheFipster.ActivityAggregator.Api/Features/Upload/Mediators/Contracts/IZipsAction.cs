using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;

namespace TheFipster.ActivityAggregator.Api.Features.Upload.Mediators.Contracts;

public interface IZipsAction
{
    PagedResponse<ZipIndex> GetZipFilePage(ZipsPageRequest request);
}
