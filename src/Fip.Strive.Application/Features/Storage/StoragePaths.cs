namespace Fip.Strive.Application.Features.Storage;

/// <summary>
/// The resolved on-disk layout. Built once at startup so nothing downstream has to reason about
/// relative-vs-absolute configuration again.
/// </summary>
public sealed class StoragePaths
{
    public StoragePaths(string root)
    {
        Root = root;
        Blobs = Path.Combine(root, "blobs");
        Incoming = Path.Combine(root, "incoming");
    }

    /// <summary>Configured data directory, always absolute.</summary>
    public string Root { get; }

    /// <summary>Content-addressed blob store, sharded by hash prefix.</summary>
    public string Blobs { get; }

    /// <summary>Staging area for uploads that have not been imported yet.</summary>
    public string Incoming { get; }

    public void EnsureCreated()
    {
        Directory.CreateDirectory(Blobs);
        Directory.CreateDirectory(Incoming);
    }
}
