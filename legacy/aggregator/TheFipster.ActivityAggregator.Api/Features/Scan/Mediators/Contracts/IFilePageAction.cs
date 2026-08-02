using TheFipster.ActivityAggregator.Domain.Models.Indexes;
using TheFipster.ActivityAggregator.Domain.Models.Requests;

namespace TheFipster.ActivityAggregator.Api.Features.Scan.Mediators.Contracts;

public interface IFilePageAction
{
    PagedResponse<FileIndex> GetFilePage(ScanFilePageRequest request);
}
