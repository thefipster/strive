using Fip.Strive.Core.Domain.Schemas.Index.Models;
using LiteDB;

namespace Fip.Strive.Harvester.Application.Infrastructure.Contexts.Config;

public static class IndexerMapper
{
    public static BsonMapper Mapper
    {
        get
        {
            var mapper = BsonMapper.Global;
            mapper.Entity<ZipIndex>().Id(x => x.Hash);

            return mapper;
        }
    }
}
