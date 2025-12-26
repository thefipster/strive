using Fip.Strive.Core.Domain.Schemas.Requests.Paging;
using Fip.Strive.Harvester.Application.Infrastructure.Indexing.Data;

namespace Fip.Strive.Harvester.Application.Infrastructure.Indexing.Repositories.Contracts;

public interface IFileRepository
{
    Task<int> BulkInsert(CancellationToken ct, List<FileInstance> items);

    Task<FileInstance?> GetByHash(string hash);
    Task<FileInstance?> GetByFile(string filepath);
    Task<PagedResponse<FileInstance>> GetPageAsync(
        int pageNumber,
        int pageSize,
        CancellationToken ct
    );

    Task<PagedResponse<FileInstance>> GetPageAsync(
        string parentFile,
        int pageNumber,
        int pageSize,
        CancellationToken ct
    );
}
