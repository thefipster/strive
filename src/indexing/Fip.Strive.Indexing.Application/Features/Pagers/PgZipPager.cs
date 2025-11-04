using Fip.Strive.Indexing.Application.Infrastructure.Postgres.Contexts;
using Fip.Strive.Indexing.Application.Infrastructure.Postgres.Repositories;
using Fip.Strive.Indexing.Domain;

namespace Fip.Strive.Indexing.Application.Features.Pagers;

public class PgZipPager(IndexPgContext context) : EfCoreSpecificationReader<ZipIndex>(context.Zips);
