using Fip.Strive.Core.Application.Infrastructure.Contexts;
using Fip.Strive.Indexing.Application.Infrastructure.Lite.Config;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Indexing.Application.Infrastructure.Lite.Contexts;

public class IndexContext(IOptions<IndexConfig> config)
    : LiteBaseContext(config.Value.DatabasePath, IndexerMapper.Mapper) { }
