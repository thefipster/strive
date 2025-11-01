using Fip.Strive.Core.Domain.Schemas.Index.Models;
using Fip.Strive.Harvester.Application.Infrastructure.Contexts;
using Fip.Strive.Harvester.Application.Infrastructure.Repositories;

namespace Fip.Strive.Harvester.Application.Core.Indexing.Pagers;

public class ZipPager(IndexContext context)
    : SpecificationReader<ZipIndex>(context.GetCollection<ZipIndex>());
