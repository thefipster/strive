using Fip.Strive.Core.Domain.Exceptions;
using LiteDB;

namespace Fip.Strive.Core.Application.Infrastructure.Contexts;

public class LiteBaseContext : IDisposable
{
    private readonly LiteDatabase _database;

    public LiteBaseContext(string filepath, BsonMapper? mapper = null)
    {
        var dir = ValidateDirectoryPath(filepath);
        EnsureDirectory(dir);

        if (mapper == null)
            _database = new LiteDatabase(filepath);
        else
            _database = new LiteDatabase(filepath, mapper);
    }

    public ILiteCollection<T> GetCollection<T>() => _database.GetCollection<T>(typeof(T).Name);

    public IEnumerable<string> GetCollectionNames() => _database.GetCollectionNames();

    public void Rebuild() => _database.Rebuild();

    public void Dispose() => _database.Dispose();

    private static DirectoryInfo ValidateDirectoryPath(string filepath)
    {
        var file = new FileInfo(filepath);
        if (file.Directory == null)
            throw new ConfigurationException(
                nameof(filepath),
                $"Path {filepath} has no directory, I guess. Sound weird I know."
            );

        return file.Directory;
    }

    private static void EnsureDirectory(DirectoryInfo directory)
    {
        try
        {
            if (!directory.Exists)
                directory.Create();
        }
        catch (Exception e)
        {
            throw new ConfigurationException(
                $"Could not create directory {directory.FullName} for database.",
                e
            );
        }
    }
}
