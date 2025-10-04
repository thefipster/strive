using TheFipster.ActivityAggregator.Domain.Tools;

namespace TheFipster.ActivityAggregator.Importer.UnitTests.Fixtures;

public class TestFileFixture : IDisposable
{
    public string DataRoot { get; } =
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "data"));

    public string GetVendorPath(string vendor) => Path.Combine(DataRoot, vendor);

    public string GetFilePath(string vendor, string fileName) =>
        Path.Combine(GetVendorPath(vendor), fileName);

    public FileProbe GetFileProbe(string vendor, string fileName) =>
        new FileProbe(GetFilePath(vendor, fileName));

    public void Dispose()
    {
        // Usually not needed, but useful if you create temp files etc.
    }
}
