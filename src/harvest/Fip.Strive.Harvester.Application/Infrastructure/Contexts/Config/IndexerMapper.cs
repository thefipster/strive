using Fip.Strive.Core.Domain.Schemas.Index.Models;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
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
            mapper.Entity<FileIndex>().Id(x => x.Hash);
            mapper.Entity<DataIndex>().Id(x => x.Hash);
            mapper.Entity<DateEntry>().Id(x => x.Timestamp);

            return mapper;
        }
    }
}
