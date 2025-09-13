using LiteDB;

namespace Fipster.TrackMe.Storage.Lite.Abstraction
{
    public interface ILiteDbService
    {
        ILiteCollection<T> GetCollection<T>(string? name = null);
    }
}
