using AwesomeAssertions;
using Fip.Strive.Application.Features.Storage;
using Fip.Strive.Application.UnitTests.Fixtures;

namespace Fip.Strive.Application.UnitTests.Storage;

public class StoragePathsTests
{
    [Fact]
    public void Derives_blobs_and_incoming_from_the_root()
    {
        var root = Path.Combine(Path.GetTempPath(), "strive-data");

        var paths = new StoragePaths(root);

        paths.Root.Should().Be(root);
        paths.Blobs.Should().Be(Path.Combine(root, "blobs"));
        paths.Incoming.Should().Be(Path.Combine(root, "incoming"));
    }

    [Fact]
    public void EnsureCreated_creates_both_directories()
    {
        using var temp = new TempDirectory();
        var paths = new StoragePaths(Path.Combine(temp.Path, "data"));

        paths.EnsureCreated();

        Directory.Exists(paths.Blobs).Should().BeTrue();
        Directory.Exists(paths.Incoming).Should().BeTrue();
    }

    [Fact]
    public void A_relative_data_directory_resolves_against_the_base_path()
    {
        // Mirrors how Registration resolves StorageOptions.DataDirectory.
        var basePath = Path.Combine(Path.GetTempPath(), "content-root");

        var resolved = Path.GetFullPath("data", basePath);

        resolved.Should().Be(Path.Combine(basePath, "data"));
    }

    [Fact]
    public void An_absolute_data_directory_ignores_the_base_path()
    {
        var basePath = Path.Combine(Path.GetTempPath(), "content-root");
        var absolute = Path.Combine(Path.GetTempPath(), "elsewhere", "strive");

        var resolved = Path.GetFullPath(absolute, basePath);

        resolved.Should().Be(absolute);
    }
}
