using Fip.Strive.Core.Domain.Schemas.Requests.Paging;
using Fip.Strive.Harvester.Application.Infrastructure.Indexing.Data;

namespace Fip.Strive.Harvester.Application.Infrastructure.Indexing.Repositories.Contracts;

public interface IZipRepository
{
    Task<int> BulkInsert(CancellationToken ct, List<ZipIndex> items);

    Task<ZipIndex?> GetByHash(string hash);
    Task<ZipIndex?> GetByFile(string filepath);
    Task<PagedResponse<ZipIndex>> GetPageAsync(int pageNumber, int pageSize, CancellationToken ct);
}
