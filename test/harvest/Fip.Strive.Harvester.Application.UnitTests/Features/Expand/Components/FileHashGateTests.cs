using AwesomeAssertions;
using Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;
using Fip.Strive.Core.Domain.Schemas.Index.Models;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Harvester.Application.Features.Expand.Component;
using Fip.Strive.Harvester.Application.Features.Expand.Models;
using Fip.Strive.Harvester.Application.Features.Expand.Repositories.Contracts;
using NSubstitute;

namespace Fip.Strive.Harvester.Application.UnitTests.Features.Expand.Components;

public class FileHashGateTests
{
    private readonly IFileIndexer _indexer;
    private readonly IFileHasher _hasher;
    private readonly FileHashGate _sut;

    public FileHashGateTests()
    {
        _indexer = Substitute.For<IFileIndexer>();
        _hasher = Substitute.For<IFileHasher>();
        _sut = new FileHashGate(_indexer, _hasher);
    }

    [Fact]
    public async Task CheckFileAsync_WhenIndexExists_ShouldAddFileToExistingIndexAndReturnIt()
    {
        // Arrange
        var filepath = @"C:\temp\somefile.txt";
        var filename = "somefile.txt";
        var hash = "abc123hash";
        var referenceId = Guid.NewGuid();
        var signal = new ImportSignal
        {
            Filepath = "archive.zip",
            ReferenceId = referenceId,
            EmittedAt = DateTime.UtcNow,
            Id = Guid.NewGuid(),
        };
        var work = WorkItem.FromSignal(signal);

        var existingIndex = new FileIndex
        {
            Hash = hash,
            ReferenceId = referenceId,
            SignalledAt = signal.EmittedAt,
            SignalId = signal.Id,
        };
        existingIndex.AddFile("otherfile.txt");

        _hasher.HashXx3Async(filepath, Arg.Any<CancellationToken>()).Returns(hash);
        _indexer.Find(hash).Returns(existingIndex);

        // Act
        var result = await _sut.CheckFileAsync(work, filepath, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(existingIndex);
        result.Files.Should().ContainKey(filename);
        result.Files.Should().HaveCount(2);
        _indexer.Received(1).Upsert(existingIndex);
    }

    [Fact]
    public async Task CheckFileAsync_WhenIndexDoesNotExist_ShouldCreateNewIndexAndReturnIt()
    {
        // Arrange
        var filepath = @"C:\temp\newfile.txt";
        var filename = "newfile.txt";
        var hash = "xyz789hash";
        var referenceId = Guid.NewGuid();
        var signalId = Guid.NewGuid();
        var emittedAt = new DateTime(2023, 5, 10, 10, 30, 0, DateTimeKind.Utc);
        var signal = new ImportSignal
        {
            Filepath = "archive.zip",
            ReferenceId = referenceId,
            EmittedAt = emittedAt,
            Id = signalId,
        };
        var work = WorkItem.FromSignal(signal);

        _hasher.HashXx3Async(filepath, Arg.Any<CancellationToken>()).Returns(hash);
        _indexer.Find(hash).Returns((FileIndex?)null);

        // Act
        var result = await _sut.CheckFileAsync(work, filepath, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Hash.Should().Be(hash);
        result.ReferenceId.Should().Be(referenceId);
        result.SignalId.Should().Be(signalId);
        result.SignalledAt.Should().Be(emittedAt);
        result.Files.Should().ContainKey(filename);
        result.Files.Should().HaveCount(1);
        _indexer
            .Received(1)
            .Upsert(Arg.Is<FileIndex>(i => i.Hash == hash && i.Files.ContainsKey(filename)));
    }

    [Fact]
    public async Task CheckFileAsync_ShouldHashFileWithCorrectPath()
    {
        // Arrange
        var filepath = @"C:\temp\testfile.txt";
        var hash = "hashvalue";
        var signal = new ImportSignal
        {
            Filepath = "archive.zip",
            ReferenceId = Guid.NewGuid(),
            EmittedAt = DateTime.UtcNow,
            Id = Guid.NewGuid(),
        };
        var work = WorkItem.FromSignal(signal);
        var cancellationToken = new CancellationToken();

        _hasher.HashXx3Async(filepath, cancellationToken).Returns(hash);
        _indexer.Find(hash).Returns((FileIndex?)null);

        // Act
        await _sut.CheckFileAsync(work, filepath, cancellationToken);

        // Assert
        await _hasher.Received(1).HashXx3Async(filepath, cancellationToken);
    }

    [Fact]
    public async Task CheckFileAsync_ShouldExtractFilenameFromPath()
    {
        // Arrange
        var filepath = @"C:\some\deep\directory\myfile.txt";
        var expectedFilename = "myfile.txt";
        var hash = "somehash";
        var signal = new ImportSignal
        {
            Filepath = "archive.zip",
            ReferenceId = Guid.NewGuid(),
            EmittedAt = DateTime.UtcNow,
            Id = Guid.NewGuid(),
        };
        var work = WorkItem.FromSignal(signal);

        _hasher.HashXx3Async(filepath, Arg.Any<CancellationToken>()).Returns(hash);
        _indexer.Find(hash).Returns((FileIndex?)null);

        // Act
        var result = await _sut.CheckFileAsync(work, filepath, CancellationToken.None);

        // Assert
        result.Files.Should().ContainKey(expectedFilename);
    }
}
