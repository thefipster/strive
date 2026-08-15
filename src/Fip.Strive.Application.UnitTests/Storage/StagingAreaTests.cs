using System.Security.Cryptography;
using AwesomeAssertions;
using Fip.Strive.Application.Features.Storage;
using Fip.Strive.Application.Features.Storage.Services;
using Fip.Strive.Application.UnitTests.Fixtures;
using Microsoft.Extensions.Logging.Abstractions;

namespace Fip.Strive.Application.UnitTests.Storage;

public class StagingAreaTests : IDisposable
{
    private readonly TempDirectory _root = new();
    private readonly StoragePaths _paths;
    private readonly StagingArea _staging;

    public StagingAreaTests()
    {
        _paths = new StoragePaths(_root.Path);
        _paths.EnsureCreated();
        _staging = new StagingArea(_paths, NullLogger<StagingArea>.Instance);
    }

    [Fact]
    public async Task Stages_the_archive_and_hashes_its_bytes()
    {
        var content = RandomNumberGenerator.GetBytes(256 * 1024);

        var staged = await _staging.StageAsync("takeout.zip", new MemoryStream(content));

        staged.FileName.Should().Be("takeout.zip");
        staged.SizeBytes.Should().Be(content.Length);
        staged.Hash.Should().Be(Convert.ToHexStringLower(SHA256.HashData(content)));
        File.ReadAllBytes(staged.Path).Should().Equal(content);
        staged.Path.Should().StartWith(_paths.Incoming);
    }

    [Fact]
    public async Task Reports_progress_up_to_the_full_size()
    {
        var content = RandomNumberGenerator.GetBytes(512 * 1024);
        var reported = new List<long>();

        var staged = await _staging.StageAsync(
            "takeout.zip",
            new MemoryStream(content),
            new Progress<long>(reported.Add)
        );

        // Progress<T> dispatches asynchronously, so wait for the final report rather than
        // assuming it has landed by the time StageAsync returns.
        await WaitUntil(() => reported.Contains(staged.SizeBytes));

        reported.Should().BeInAscendingOrder();
        reported.Should().AllSatisfy(value => value.Should().BeLessThanOrEqualTo(content.Length));
    }

    [Fact]
    public async Task Discard_removes_the_staged_file()
    {
        var staged = await _staging.StageAsync("takeout.zip", new MemoryStream([1, 2, 3]));

        _staging.Discard(staged);

        File.Exists(staged.Path).Should().BeFalse();
    }

    [Fact]
    public async Task Discarding_twice_is_harmless()
    {
        var staged = await _staging.StageAsync("takeout.zip", new MemoryStream([1, 2, 3]));

        _staging.Discard(staged);
        var act = () => _staging.Discard(staged);

        act.Should().NotThrow();
    }

    [Fact]
    public async Task Staging_the_same_archive_twice_reuses_the_one_file()
    {
        var content = RandomNumberGenerator.GetBytes(64 * 1024);

        var first = await _staging.StageAsync("takeout.zip", new MemoryStream(content));
        var second = await _staging.StageAsync("takeout-again.zip", new MemoryStream(content));

        // Named after its content, so an archive uploaded twice cannot leave the first copy
        // behind as an orphan nothing knows how to find.
        second.Path.Should().Be(first.Path);
        File.ReadAllBytes(first.Path).Should().Equal(content);

        Directory.EnumerateFiles(_paths.Incoming).Should().ContainSingle();
    }

    [Fact]
    public async Task Leaves_nothing_behind_when_the_source_stream_fails()
    {
        var act = async () => await _staging.StageAsync("broken.zip", new ThrowingStream());

        await act.Should().ThrowAsync<IOException>();
        Directory.EnumerateFiles(_paths.Incoming).Should().BeEmpty();
    }

    private static async Task WaitUntil(Func<bool> condition)
    {
        for (var attempt = 0; attempt < 50 && !condition(); attempt++)
            await Task.Delay(20);
    }

    public void Dispose() => _root.Dispose();

    private sealed class ThrowingStream : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count) =>
            throw new IOException("connection reset");

        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default
        ) => throw new IOException("connection reset");

        public override void Flush() { }

        public override long Seek(long offset, SeekOrigin origin) =>
            throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();
    }
}
