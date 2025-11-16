using Fip.Strive.Core.Application.Infrastructure.Contracts;
using Fip.Strive.Core.Domain.Schemas.Requests.Paging;
using Fip.Strive.Indexing.Domain.Models;
using Fip.Strive.Indexing.Storage.Postgres.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Fip.Strive.Indexing.Storage.Postgres.Repositories;

public class PgFilePager(IndexPgContext context) : ISpecificationReader<FileIndex>
{
    public PagedResponse<FileIndex> GetPaged(
        PageSpecificationRequest<FileIndex> specifications,
        CancellationToken ct = default
    )
    {
        var query = context.Files.AsNoTracking().Include(x => x.Files).AsQueryable();

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

        return new PagedResponse<FileIndex>(items, totalItems);
    }
}
