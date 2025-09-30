using LiteDB;
using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Domain.Exceptions;
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

            if (file.Directory == null)
                throw new ConfigException(
                    "Indexer database file",
                    "Directory was null??? Didn't know that could happen..."
                );

            if (!Directory.Exists(file.Directory.FullName))
                Directory.CreateDirectory(file.Directory.FullName);

            database = new LiteDatabase(file.FullName, Mapper);
        }

        public ILiteCollection<T> GetCollection<T>() => database.GetCollection<T>(typeof(T).Name);

        public ILiteCollection<T> GetCollection<T>(string name) => database.GetCollection<T>(name);

        public void Dispose() => database.Dispose();

        private BsonMapper Mapper
        {
            get
            {
                var mapper = BsonMapper.Global;

                // old school
                mapper.Entity<ImportIndex>().Id(x => x.OutputDirectory);
                mapper.Entity<ScanIndex>().Id(x => x.Filepath);
                mapper.Entity<ClassificationIndex>().Id(x => x.Filepath);
                mapper.Entity<TransformIndex>().Id(x => x.Filepath);
                mapper.Entity<BundleIndex>().Id(x => x.Id);
                mapper.Entity<UnifyIndex>().Id(x => x.Id);

                // new school
                mapper.Entity<ImporterIndex>().Id(x => x.Hash);
                mapper.Entity<ScannerIndex>().Id(x => x.Hash);
                mapper.Entity<AssimilaterIndex>().Id(x => x.FileHash);
                mapper.Entity<ConvergeIndex>().Id(x => x.FileHash);
                mapper.Entity<InventoryIndex>().Id(x => x.Timestamp);

                // totatlly new school
                mapper.Entity<ZipIndex>().Id(x => x.Hash);
                mapper.Entity<FileIndex>().Id(x => x.Hash);

                return mapper;
            }
        }
    }
}
