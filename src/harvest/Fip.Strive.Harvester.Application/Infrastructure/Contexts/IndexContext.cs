using Fip.Strive.Harvester.Application.Core.Indexing;
using Fip.Strive.Harvester.Application.Infrastructure.Contexts.Base;
using Fip.Strive.Harvester.Application.Infrastructure.Contexts.Config;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Harvester.Application.Infrastructure.Contexts;

public class IndexContext(IOptions<IndexConfig> config)
    : SimpleContext(config.Value.DatabasePath, IndexerMapper.Mapper) { }
