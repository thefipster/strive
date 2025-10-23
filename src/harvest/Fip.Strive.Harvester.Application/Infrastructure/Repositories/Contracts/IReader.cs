using Fip.Strive.Harvester.Application.Infrastructure.Models;

namespace Fip.Strive.Harvester.Application.Infrastructure.Repositories.Contracts;

public interface IReader<TEntity>
{
    PagedResponse<TEntity> GetPaged(
        PageSpecificationRequest<TEntity> specifications,
        CancellationToken ct = default
    );
}
