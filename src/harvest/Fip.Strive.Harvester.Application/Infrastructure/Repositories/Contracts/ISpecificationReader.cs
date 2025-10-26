using Fip.Strive.Harvester.Application.Infrastructure.Models;

namespace Fip.Strive.Harvester.Application.Infrastructure.Repositories.Contracts;

public interface ISpecificationReader<TEntity>
{
    PagedResponse<TEntity> GetPaged(
        PageSpecificationRequest<TEntity> specifications,
        CancellationToken ct = default
    );
}
