using LiteDB;
using Microsoft.Extensions.Options;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models;

namespace TheFipster.ActivityAggregator.Storage.Lite.Context;

public class ActivityContext : IDisposable
{
    private readonly LiteDatabase database;

    public ActivityContext(IOptions<LiteDbConfig> options)
    {
        var config = options.Value;

        var file = new FileInfo(config.ActivityFile);

        if (file.Directory == null)
            throw new ConfigException("Activity database file", "Directory was null???");

        if (!Directory.Exists(file.Directory.FullName))
            Directory.CreateDirectory(file.Directory.FullName);

        database = new LiteDatabase(file.FullName);
    }

    public ILiteCollection<T> GetCollection<T>() => database.GetCollection<T>(typeof(T).Name);

    public ILiteCollection<T> GetCollection<T>(string name) => database.GetCollection<T>(name);

    public void Dispose() => database.Dispose();

    private BsonMapper Mapper
    {
        get
        {
            var mapper = BsonMapper.Global;

            mapper.Entity<MergedRecord>().Id(x => x.Timestamp);

            return mapper;
        }
    }
}
