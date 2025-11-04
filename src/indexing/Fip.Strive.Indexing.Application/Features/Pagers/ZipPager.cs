using Fip.Strive.Indexing.Application.Infrastructure.Lite.Contexts;
using Fip.Strive.Indexing.Application.Infrastructure.Lite.Repositories;
using Fip.Strive.Indexing.Domain;

namespace Fip.Strive.Indexing.Application.Features.Pagers;

public class ZipPager(IndexContext context)
    : SpecificationReader<ZipIndex>(context.GetCollection<ZipIndex>());
