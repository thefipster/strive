using Fip.Strive.Core.Domain.Schemas.Requests.Paging;

namespace Fip.Strive.Core.Application.Infrastructure.Contracts;

public interface ISpecificationReader<TEntity>
{
    PagedResponse<TEntity> GetPaged(
        PageSpecificationRequest<TEntity> specifications,
        CancellationToken ct = default
    );
}
