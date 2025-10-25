using Fip.Strive.Core.Domain.Schemas.Index.Models;
using Fip.Strive.Harvester.Application.Infrastructure.Contexts;
using Fip.Strive.Harvester.Application.Infrastructure.Repositories;

namespace Fip.Strive.Harvester.Application.Features.Expand.Repositories;

public class FilePager(IndexContext context)
    : SpecificationReader<FileIndex>(context.GetCollection<FileIndex>()) { }
