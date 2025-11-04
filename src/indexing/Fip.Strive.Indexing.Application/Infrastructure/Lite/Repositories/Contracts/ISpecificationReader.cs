using Fip.Strive.Core.Domain.Schemas.Requests.Paging;

namespace Fip.Strive.Indexing.Application.Infrastructure.Lite.Repositories.Contracts;

public interface ISpecificationReader<TEntity>
{
    PagedResponse<TEntity> GetPaged(
        PageSpecificationRequest<TEntity> specifications,
        CancellationToken ct = default
    );
}
