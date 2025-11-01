using Fip.Strive.Core.Domain.Schemas.Index.Models;
using Fip.Strive.Harvester.Application.Infrastructure.Contexts;
using Fip.Strive.Harvester.Application.Infrastructure.Repositories;

namespace Fip.Strive.Harvester.Application.Features.Assimilate.Repositories;

public class DataPager(IndexContext context)
    : SpecificationReader<DataIndex>(context.GetCollection<DataIndex>());
