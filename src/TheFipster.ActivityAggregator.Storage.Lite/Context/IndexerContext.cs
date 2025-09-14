using LiteDB;
using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Models.Indexes;

namespace TheFipster.ActivityAggregator.Storage.Lite.Context
{
    public class IndexerContext : IDisposable
    {
        private readonly LiteDatabase database;

        public IndexerContext(IOptions<LiteDbConfig> options)
        {
            var config = options.Value;

            var file = new FileInfo(config.IndexFile);

            if (!Directory.Exists(file.Directory.FullName))
                Directory.CreateDirectory(file.Directory.FullName);

            database = new LiteDatabase(file.FullName, Mapper);
        }

        public ILiteCollection<ClassificationIndex> GetScanCollection() =>
            database.GetCollection<ClassificationIndex>("ScanIndex");

        public ILiteCollection<TransformIndex> GetTransformCollection() =>
            database.GetCollection<TransformIndex>("TransformIndex");

        public void Dispose() => database?.Dispose();

        private BsonMapper Mapper
        {
            get
            {
                var mapper = BsonMapper.Global;

                mapper.Entity<UnifiedRecord>().Id(x => x.Timestamp);
                mapper.Entity<ClassificationIndex>().Id(x => x.Filepath);
                mapper.Entity<TransformIndex>().Id(x => x.Filepath);
                mapper.Entity<ImportIndex>().Id(x => x.Filepath);

                return mapper;
            }
        }
    }
}
