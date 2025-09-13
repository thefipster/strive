using Fipster.TrackMe.Domain.Models;
using Fipster.TrackMe.Storage.Lite.Abstraction;
using LiteDB;
using Microsoft.Extensions.Options;

namespace Fipster.TrackMe.Storage.Lite.Context
{
    public class LiteDbIndexContext : ILiteDbService, IDisposable
    {
        private readonly LiteDatabase database;

        public LiteDbIndexContext(IOptions<LiteDbConfig> options)
        {
            var config = options.Value;
            database = new LiteDatabase(config.IndexFile, Mapper);
        }

        public ILiteCollection<T> GetCollection<T>(string? name = null)
        {
            // Use class name if no custom name is provided
            name ??= typeof(T).Name;
            return database.GetCollection<T>(name);
        }

        public void Dispose()
        {
            database?.Dispose();
        }

        public static BsonMapper Mapper
        {
            get
            {
                var mapper = BsonMapper.Global;
                mapper.Entity<UnifiedRecord>().Id(x => x.Timestamp);
                return mapper;
            }
        }
    }
}
