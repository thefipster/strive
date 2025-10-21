using Fip.Strive.Harvester.Application.Infrastructure.Config;
using Fip.Strive.Harvester.Application.Infrastructure.Contexts.Base;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Harvester.Application.Infrastructure.Contexts;

public class IndexContext(IOptions<IndexConfig> config)
    : SimpleContext(config.Value.DatabasePath, IndexerMapper.Mapper) { }
