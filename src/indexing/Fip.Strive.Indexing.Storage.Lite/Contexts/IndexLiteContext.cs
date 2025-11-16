using Fip.Strive.Core.Application.Infrastructure.Contexts;
using Fip.Strive.Indexing.Storage.Lite.Model;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Indexing.Storage.Lite.Contexts;

public class IndexLiteContext(IOptions<LiteDbConfig> config)
    : LiteBaseContext(config.Value.DatabasePath, IndexerMapper.Mapper) { }
