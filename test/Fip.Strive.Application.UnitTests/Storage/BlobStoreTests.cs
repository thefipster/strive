using System.Security.Cryptography;
using System.Text;
using AwesomeAssertions;
using Fip.Strive.Application.Features.Storage;
using Fip.Strive.Application.Features.Storage.Services;
using Fip.Strive.Application.UnitTests.Fixtures;

namespace Fip.Strive.Application.UnitTests.Storage;

public class BlobStoreTests : IDisposable
{
    private readonly TempDirectory _root = new();
    private readonly StoragePaths _paths;
    private readonly BlobStore _store;

    public BlobStoreTests()
    {
        _paths = new StoragePaths(_root.Path);
        _paths.EnsureCreated();
        _store = new BlobStore(_paths);
    }

    [Fact]
    public async Task Writes_content_under_its_sha256()
    {
        var content = "polar flow activity"u8.ToArray();

        var result = await _store.WriteAsync(new MemoryStream(content));

        result.Hash.Should().Be(Sha256Of(content));
        result.SizeBytes.Should().Be(content.Length);
        result.WasWritten.Should().BeTrue();
        File.ReadAllBytes(_store.GetPath(result.Hash)).Should().Equal(content);
    }

    [Fact]
    public async Task Shards_the_path_by_hash_prefix()
    {
        var result = await _store.WriteAsync(new MemoryStream("shard me"u8.ToArray()));

        var hash = result.Hash;
        var expected = Path.Combine(_paths.Blobs, hash[..2], hash[2..4], hash);

        _store.GetPath(hash).Should().Be(expected);
        File.Exists(expected).Should().BeTrue();
    }

    [Fact]
    public async Task Writes_identical_content_only_once()
    {
        var content = "the same bytes"u8.ToArray();

        var first = await _store.WriteAsync(new MemoryStream(content));
        var second = await _store.WriteAsync(new MemoryStream(content));

        second.Hash.Should().Be(first.Hash);
        first.WasWritten.Should().BeTrue();
        second.WasWritten.Should().BeFalse("identical content must not be stored twice");
        CountBlobs().Should().Be(1);
    }

    [Fact]
    public async Task Keeps_different_content_apart()
    {
        var first = await _store.WriteAsync(new MemoryStream("one"u8.ToArray()));
        var second = await _store.WriteAsync(new MemoryStream("two"u8.ToArray()));

        second.Hash.Should().NotBe(first.Hash);
        CountBlobs().Should().Be(2);
    }

    [Fact]
    public async Task Handles_empty_content()
    {
        var result = await _store.WriteAsync(new MemoryStream([]));

        result.SizeBytes.Should().Be(0);
        result.Hash.Should().Be(Sha256Of([]));
        _store.Exists(result.Hash).Should().BeTrue();
    }

    [Fact]
    public async Task Hashes_content_larger_than_the_read_buffer()
    {
        // 1 MB exercises the multi-chunk path through the incremental hasher.
        var content = RandomNumberGenerator.GetBytes(1024 * 1024);

        var result = await _store.WriteAsync(new MemoryStream(content));

        result.Hash.Should().Be(Sha256Of(content));
        result.SizeBytes.Should().Be(content.Length);
    }

    [Fact]
    public async Task Leaves_no_temporary_files_behind()
    {
        await _store.WriteAsync(new MemoryStream("first"u8.ToArray()));
        await _store.WriteAsync(new MemoryStream("first"u8.ToArray()));

        var tempDirectory = Path.Combine(_paths.Blobs, ".tmp");

        Directory.EnumerateFiles(tempDirectory).Should().BeEmpty();
    }

    [Fact]
    public async Task Round_trips_through_OpenRead()
    {
        var content = "read me back"u8.ToArray();
        var result = await _store.WriteAsync(new MemoryStream(content));

        await using var stream = _store.OpenRead(result.Hash);
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer);

        buffer.ToArray().Should().Equal(content);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    public void Rejects_hashes_it_could_not_have_produced(string hash)
    {
        var act = () => _store.GetPath(hash);

        act.Should().Throw<ArgumentException>();
    }

    private int CountBlobs() =>
        Directory
            .EnumerateFiles(_paths.Blobs, "*", SearchOption.AllDirectories)
            .Count(path => !path.Contains(".tmp", StringComparison.Ordinal));

    private static string Sha256Of(byte[] content) =>
        Convert.ToHexStringLower(SHA256.HashData(content));

    public void Dispose() => _root.Dispose();
}
