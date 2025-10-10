using TheFipster.ActivityAggregator.Api.Models.Requests;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;

namespace TheFipster.ActivityAggregator.Api.Mediators.Scan.Contracts;

public interface IFilesAction
{
    PagedResult<FileIndex> GetFilePage(ScanFilePageRequest request);
}
