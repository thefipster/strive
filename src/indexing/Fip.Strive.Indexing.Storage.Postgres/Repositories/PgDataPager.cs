using Fip.Strive.Indexing.Domain.Models;
using Fip.Strive.Indexing.Storage.Postgres.Contexts;

namespace Fip.Strive.Indexing.Storage.Postgres.Repositories;

public class PgDataPager(IndexPgContext context)
    : EfCoreSpecificationReader<DataIndex>(context.Data);
