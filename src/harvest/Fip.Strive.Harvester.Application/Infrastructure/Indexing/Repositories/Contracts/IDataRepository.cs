using Fip.Strive.Core.Domain.Schemas.Requests.Paging;
using Fip.Strive.Harvester.Application.Infrastructure.Indexing.Data;

namespace Fip.Strive.Harvester.Application.Infrastructure.Indexing.Repositories.Contracts;

public interface IDataRepository
{
    Task<int> BulkInsert(CancellationToken ct, List<DataIndex> items);

    Task<PagedResponse<DataIndex>> GetPageAsync(
        string parentFile,
        int pageNumber,
        int pageSize,
        CancellationToken ct
    );
}
