using LiteDB;

namespace TheFipster.ActivityAggregator.Storage.Lite.Abstraction
{
    public interface ILiteDbService
    {
        ILiteCollection<T> GetCollection<T>(string? name = null);
    }
}
