using Fip.Strive.Indexing.Domain.Models;
using LiteDB;

namespace Fip.Strive.Indexing.Storage.Lite.Model;

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

            mapper.Entity<ZipIndexV2>().Id(x => x.Filepath);
            mapper.Entity<FileIndexV2>().Id(x => x.Filepath);
            mapper.Entity<TypeIndexV2>().Id(x => x.Hash);
            mapper.Entity<AssimilateIndexV2>().Id(x => x.Hash);
            mapper.Entity<ExtractIndexV2>().Id(x => x.Filepath);

            mapper.Entity<DateEntry>().Id(x => x.Timestamp);

            return mapper;
        }
    }
}
