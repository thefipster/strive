using AwesomeAssertions;
using Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;
using Fip.Strive.Core.Domain.Schemas.Index.Models;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Harvester.Application.Core.Indexing.Contracts;
using Fip.Strive.Harvester.Application.Features.Import.Components.Contracts;
using Fip.Strive.Harvester.Application.Features.Import.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fip.Strive.Harvester.Application.UnitTests.Features.Import.Services;

public class ZipInventoryTests
{
    private readonly IIndexer<ZipIndex, string> _indexer;
    private readonly IZipFileAccess _fileAccess;
    private readonly IFileHasher _hasher;
    private readonly ILogger<ZipInventory> _logger;
    private readonly ZipInventory _sut;
    private readonly string _testRootPath;

    public ZipInventoryTests()
    {
        _indexer = Substitute.For<IIndexer<ZipIndex, string>>();
        _fileAccess = Substitute.For<IZipFileAccess>();
        _hasher = Substitute.For<IFileHasher>();
        _logger = Substitute.For<ILogger<ZipInventory>>();
        _sut = new ZipInventory(_indexer, _fileAccess, _logger);
        _testRootPath = Path.Combine(Path.GetTempPath(), $"ZipInventoryTests_{Guid.NewGuid()}");
    }

    [Fact]
    public async Task ImportAsync_WhenFileIsNew_ShouldHashImportAndUpsertIndex()
    {
        // Arrange
        var filepath = Path.Combine(_testRootPath, "newfile.zip");
        var hash = "newhash123";
        var uploadSignal = UploadSignal.From(filepath, hash);
        var importedPath = "files/import/newfile.zip";

        _hasher.HashXx3Async(filepath, Arg.Any<CancellationToken>()).Returns(hash);
        _indexer.Find(hash).Returns((ZipIndex?)null);
        _fileAccess.Import(filepath).Returns(importedPath);

        // Act
        var result = await _sut.ImportAsync(uploadSignal, CancellationToken.None);

        // Assert
        result.ImportedPath.Should().Be(importedPath);
        result.Skip.Should().BeFalse();
        _indexer
            .Received(1)
            .Upsert(Arg.Is<ZipIndex>(i => i.Hash == hash && i.Files.ContainsKey("newfile.zip")));
    }

    [Fact]
    public async Task ImportAsync_WhenFileIsAlreadyIndexed_ShouldSkipAndDeleteFile()
    {
        // Arrange
        var hash = "existinghash";
        var filepath = Path.Combine(_testRootPath, "duplicate.zip");
        Directory.CreateDirectory(_testRootPath);
        File.WriteAllText(filepath, "test content");

        var uploadSignal = UploadSignal.From(filepath, hash);

        var existingIndex = new ZipIndex
        {
            Hash = hash,
            ReferenceId = uploadSignal.ReferenceId,
            SignalledAt = uploadSignal.EmittedAt,
            SignalId = uploadSignal.Id,
        };
        existingIndex.AddFile("duplicate.zip");

        _hasher.HashXx3Async(filepath, Arg.Any<CancellationToken>()).Returns(hash);
        _indexer.Find(hash).Returns(existingIndex);

        try
        {
            // Act
            var result = await _sut.ImportAsync(uploadSignal, CancellationToken.None);

            // Assert
            result.Skip.Should().BeTrue();
            File.Exists(filepath).Should().BeFalse();
            _fileAccess.DidNotReceive().Import(Arg.Any<string>());
            _indexer.DidNotReceive().Upsert(Arg.Any<ZipIndex>());
            _logger.Received(1);
        }
        finally
        {
            if (Directory.Exists(_testRootPath))
                Directory.Delete(_testRootPath, true);
        }
    }

    [Fact]
    public async Task ImportAsync_WhenIndexExistsButFileIsNew_ShouldNotImportButUpsertIndex()
    {
        // Arrange
        var filepath = Path.Combine(_testRootPath, "anotherfile.zip");
        var hash = "samehash";
        var uploadSignal = UploadSignal.From(filepath, hash);

        var existingIndex = new ZipIndex
        {
            Hash = hash,
            ReferenceId = Guid.NewGuid(),
            SignalledAt = DateTime.UtcNow,
            SignalId = Guid.NewGuid(),
        };
        existingIndex.AddFile("differentfile.zip");

        _hasher.HashXx3Async(filepath, Arg.Any<CancellationToken>()).Returns(hash);
        _indexer.Find(hash).Returns(existingIndex);

        // Act
        var result = await _sut.ImportAsync(uploadSignal, CancellationToken.None);

        // Assert
        result.Skip.Should().BeFalse();
        result.ImportedPath.Should().BeNull();
        _fileAccess.DidNotReceive().Import(Arg.Any<string>());
        _indexer
            .Received(1)
            .Upsert(
                Arg.Is<ZipIndex>(i => i.Hash == hash && i.Files.ContainsKey("anotherfile.zip"))
            );
    }

    [Fact]
    public async Task ImportAsync_ShouldCreateWorkItemFromSignal()
    {
        // Arrange
        var filepath = @"C:\upload\myfile.zip";
        var hash = "hash123";
        var uploadSignal = UploadSignal.From(filepath, hash);

        _hasher.HashXx3Async(filepath, Arg.Any<CancellationToken>()).Returns(hash);
        _indexer.Find(hash).Returns((ZipIndex?)null);
        _fileAccess.Import(filepath).Returns(@"C:\import\myfile.zip");

        // Act
        var result = await _sut.ImportAsync(uploadSignal, CancellationToken.None);

        // Assert
        result.Signal.Should().BeSameAs(uploadSignal);
    }

    [Fact]
    public async Task ImportAsync_WhenIndexIsNull_ShouldCallFileAccessImport()
    {
        // Arrange
        var hash = "newhash";
        var filepath = @"C:\upload\newfile.zip";
        var uploadSignal = UploadSignal.From(filepath, hash);
        var expectedImportPath = @"C:\import\newfile.zip";

        _hasher.HashXx3Async(filepath, Arg.Any<CancellationToken>()).Returns(hash);
        _indexer.Find(hash).Returns((ZipIndex?)null);
        _fileAccess.Import(filepath).Returns(expectedImportPath);

        // Act
        await _sut.ImportAsync(uploadSignal, CancellationToken.None);

        // Assert
        _fileAccess.Received(1).Import(filepath);
    }

    [Fact]
    public async Task ImportAsync_ShouldUpsertIndexWithCorrectMetadata()
    {
        // Arrange
        var filepath = "files/upload/test.zip";
        var hash = "testhash";
        var uploadSignal = UploadSignal.From(filepath, hash);

        _hasher.HashXx3Async(filepath, Arg.Any<CancellationToken>()).Returns(hash);
        _indexer.Find(hash).Returns((ZipIndex?)null);
        _fileAccess.Import(filepath).Returns("files/import/test.zip");

        // Act
        await _sut.ImportAsync(uploadSignal, CancellationToken.None);

        // Assert
        _indexer
            .Received(1)
            .Upsert(
                Arg.Is<ZipIndex>(i =>
                    i.Hash == hash
                    && i.ReferenceId == uploadSignal.ReferenceId
                    && i.SignalId == uploadSignal.Id
                    && i.SignalledAt == uploadSignal.EmittedAt
                    && i.Files.ContainsKey("test.zip")
                )
            );
    }

    [Fact]
    public async Task ImportAsync_WhenFileIsIndexedWithDifferentCasing_ShouldStillSkip()
    {
        // Arrange
        var hash = "hash";
        var filepath = Path.Combine(_testRootPath, "TestFile.ZIP");
        Directory.CreateDirectory(_testRootPath);
        File.WriteAllText(filepath, "content");

        var uploadSignal = UploadSignal.From(filepath, hash);

        var existingIndex = new ZipIndex
        {
            Hash = hash,
            ReferenceId = uploadSignal.ReferenceId,
            SignalledAt = uploadSignal.EmittedAt,
            SignalId = uploadSignal.Id,
        };
        existingIndex.AddFile("TestFile.ZIP");

        _hasher.HashXx3Async(filepath, Arg.Any<CancellationToken>()).Returns(hash);
        _indexer.Find(hash).Returns(existingIndex);

        try
        {
            // Act
            var result = await _sut.ImportAsync(uploadSignal, CancellationToken.None);

            // Assert
            result.Skip.Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(_testRootPath))
                Directory.Delete(_testRootPath, true);
        }
    }
}
