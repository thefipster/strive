using TheFipster.ActivityAggregator.Api.Models.Requests;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;

namespace TheFipster.ActivityAggregator.Api.Abstraction;

public interface IZipsAction
{
    PagedResult<ZipIndex> TryGetZipFilePage(UploadFilePageRequest request);
}
