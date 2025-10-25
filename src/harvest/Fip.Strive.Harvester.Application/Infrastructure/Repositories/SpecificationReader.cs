using Fip.Strive.Harvester.Application.Infrastructure.Models;
using Fip.Strive.Harvester.Application.Infrastructure.Repositories.Contracts;
using LiteDB;

namespace Fip.Strive.Harvester.Application.Infrastructure.Repositories;

public class SpecificationReader<TEntity>(ILiteCollection<TEntity> collection)
    : ISpecificationReader<TEntity>
{
    public PagedResponse<TEntity> GetPaged(
        PageSpecificationRequest<TEntity> specifications,
        CancellationToken ct = default
    )
    {
        var query = collection.Query();

        foreach (var filter in specifications.Filters)
            query.Where(filter);

        if (specifications.Sort != null)
        {
            query = specifications.IsDescending
                ? query.OrderByDescending(specifications.Sort)
                : query.OrderBy(specifications.Sort);
        }
        else
        {
            query = query.OrderByDescending(x => x);
        }

        var totalItems = query.Count();
        var items = query
            .Skip(specifications.Page * specifications.Size)
            .Limit(specifications.Size)
            .ToArray();

        return new PagedResponse<TEntity>(items, totalItems);
    }
}
