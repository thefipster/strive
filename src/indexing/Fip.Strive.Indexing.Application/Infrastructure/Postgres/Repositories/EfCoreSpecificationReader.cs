using Fip.Strive.Core.Application.Infrastructure.Contracts;
using Fip.Strive.Core.Domain.Schemas.Requests.Paging;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Indexing.Application.Infrastructure.Postgres.Repositories;

public class EfCoreSpecificationReader<TEntity>(DbSet<TEntity> dbSet)
    : ISpecificationReader<TEntity>
    where TEntity : class
{
    public PagedResponse<TEntity> GetPaged(
        PageSpecificationRequest<TEntity> specifications,
        CancellationToken ct = default
    )
    {
        var query = dbSet.AsQueryable();

        foreach (var filter in specifications.Filters)
            query = query.Where(filter);

        if (specifications.Sort != null)
        {
            query = specifications.IsDescending
                ? query.OrderByDescending(specifications.Sort)
                : query.OrderBy(specifications.Sort);
        }

        var totalItems = query.Count();
        var items = query
            .Skip(specifications.Page * specifications.Size)
            .Take(specifications.Size)
            .ToArray();

        return new PagedResponse<TEntity>(items, totalItems);
    }
}
