using Fip.Strive.Harvester.Application.Features.Import.Models;
using LiteDB;

namespace Fip.Strive.Harvester.Application.Infrastructure.Config;

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
