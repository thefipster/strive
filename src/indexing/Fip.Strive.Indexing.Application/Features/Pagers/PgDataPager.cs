using Fip.Strive.Indexing.Application.Infrastructure.Postgres.Contexts;
using Fip.Strive.Indexing.Application.Infrastructure.Postgres.Repositories;
using Fip.Strive.Indexing.Domain;

namespace Fip.Strive.Indexing.Application.Features.Pagers;

public class PgDataPager(IndexPgContext context)
    : EfCoreSpecificationReader<DataIndex>(context.Data);
