using Fip.Strive.Indexing.Domain;
using Fip.Strive.Ingestion.Domain.Models;
using LiteDB;

namespace Fip.Strive.Indexing.Application.Infrastructure.Lite.Config;

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
