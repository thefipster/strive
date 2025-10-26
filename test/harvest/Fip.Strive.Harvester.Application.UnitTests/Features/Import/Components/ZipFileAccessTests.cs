using AwesomeAssertions;
using Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;
using Fip.Strive.Harvester.Application.Features.Import;
using Fip.Strive.Harvester.Application.Features.Import.Components;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Fip.Strive.Harvester.Application.UnitTests.Features.Import.Components;

public class ZipFileAccessTests
{
    private readonly IDirectoryService _directoryService;
    private readonly IFileService _fileService;
    private readonly ZipFileAccess _sut;
    private readonly string _rootPath;
    private readonly string _testUploadPath;

    public ZipFileAccessTests()
    {
        _rootPath = Path.Combine(Path.GetTempPath(), $"ImportTests_{Guid.NewGuid()}");
        _testUploadPath = Path.Combine(Path.GetTempPath(), $"UploadTests_{Guid.NewGuid()}");

        var importConfig = new ImportConfig { Path = _rootPath };
        var config = Options.Create(importConfig);

        _directoryService = Substitute.For<IDirectoryService>();
        _fileService = Substitute.For<IFileService>();

        _sut = new ZipFileAccess(config, _directoryService, _fileService);
    }

    [Fact]
    public void Import_ShouldCreateRootDirectory()
    {
        // Arrange
        var uploadPath = Path.Combine(_testUploadPath, "upload.zip");

        // Act
        _sut.Import(uploadPath);

        // Assert
        _directoryService.Received(1).CreateDirectory(_rootPath);
    }

    [Fact]
    public void Import_ShouldCopyFileToDestination()
    {
        // Arrange
        var uploadPath = Path.Combine(_testUploadPath, "upload.zip");
        var expectedDestination = Path.Combine(_rootPath, "upload.zip");

        // Act
        _sut.Import(uploadPath);

        // Assert
        _fileService.Received(1).Copy(uploadPath, expectedDestination);
    }

    [Fact]
    public void Import_ShouldDeleteUploadedFile()
    {
        // Arrange
        var uploadPath = Path.Combine(_testUploadPath, "upload.zip");

        // Act
        _sut.Import(uploadPath);

        // Assert
        _fileService.Received(1).Delete(uploadPath);
    }

    [Fact]
    public void Import_ShouldReturnDestinationPath()
    {
        // Arrange
        var uploadPath = Path.Combine(_testUploadPath, "myfile.zip");
        var expectedDestination = Path.Combine(_rootPath, "myfile.zip");

        // Act
        var result = _sut.Import(uploadPath);

        // Assert
        result.Should().Be(expectedDestination);
    }

    [Fact]
    public void Import_ShouldExtractFileNameFromUploadPath()
    {
        // Arrange
        var uploadPath = Path.Combine(_testUploadPath, "subfolder", "archive.zip");
        var expectedFileName = "archive.zip";
        var expectedDestination = Path.Combine(_rootPath, expectedFileName);

        // Act
        var result = _sut.Import(uploadPath);

        // Assert
        result.Should().Be(expectedDestination);
        _fileService
            .Received(1)
            .Copy(uploadPath, Arg.Is<string>(s => Path.GetFileName(s) == expectedFileName));
    }

    [Fact]
    public void Import_ShouldNotOverwriteExistingFiles()
    {
        // Arrange
        var uploadPath = Path.Combine(_testUploadPath, "test.zip");
        var destinationPath = Path.Combine(_rootPath, "test.zip");

        // Act
        _sut.Import(uploadPath);

        // Assert
        _fileService.Received(1).Copy(uploadPath, destinationPath, overwrite: false);
    }

    [Fact]
    public void Import_ShouldPerformOperationsInCorrectOrder()
    {
        // Arrange
        var uploadPath = Path.Combine(_testUploadPath, "test.zip");
        var callOrder = new List<string>();

        _directoryService
            .When(x => x.CreateDirectory(Arg.Any<string>()))
            .Do(_ => callOrder.Add("CreateDirectory"));

        _fileService
            .When(x => x.Copy(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>()))
            .Do(_ => callOrder.Add("Copy"));

        _fileService.When(x => x.Delete(Arg.Any<string>())).Do(_ => callOrder.Add("Delete"));

        // Act
        _sut.Import(uploadPath);

        // Assert
        callOrder.Should().ContainInOrder("CreateDirectory", "Copy", "Delete");
    }

    [Fact]
    public void Open_ShouldReturnFileStreamWithCorrectPath()
    {
        // Arrange
        var importPath = Path.Combine(_rootPath, "test.zip");
        Directory.CreateDirectory(_rootPath);
        File.WriteAllText(importPath, "test content");

        try
        {
            // Act
            using var result = _sut.Open(importPath);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(importPath);
        }
        finally
        {
            if (Directory.Exists(_rootPath))
                Directory.Delete(_rootPath, true);
        }
    }

    [Fact]
    public void Open_ShouldReturnFileStreamWithReadAccess()
    {
        // Arrange
        var importPath = Path.Combine(_rootPath, "test.zip");
        Directory.CreateDirectory(_rootPath);
        File.WriteAllText(importPath, "test content");

        try
        {
            // Act
            using var result = _sut.Open(importPath);

            // Assert
            result.CanRead.Should().BeTrue();
            result.CanWrite.Should().BeFalse();
        }
        finally
        {
            if (Directory.Exists(_rootPath))
                Directory.Delete(_rootPath, true);
        }
    }

    [Fact]
    public void Open_ShouldReturnFileStreamWithAsyncSupport()
    {
        // Arrange
        var importPath = Path.Combine(_rootPath, "test.zip");
        Directory.CreateDirectory(_rootPath);
        File.WriteAllText(importPath, "test content");

        try
        {
            // Act
            using var result = _sut.Open(importPath);

            // Assert
            result.IsAsync.Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(_rootPath))
                Directory.Delete(_rootPath, true);
        }
    }

    [Fact]
    public void Open_ShouldAllowSharedReading()
    {
        // Arrange
        var importPath = Path.Combine(_rootPath, "test.zip");
        Directory.CreateDirectory(_rootPath);
        File.WriteAllText(importPath, "test content");

        try
        {
            using var firstStream = _sut.Open(importPath);

            // Act
            var act = () => _sut.Open(importPath);

            // Assert
            using var secondStream = act();
            secondStream.Should().NotBeNull();
        }
        finally
        {
            if (Directory.Exists(_rootPath))
                Directory.Delete(_rootPath, true);
        }
    }
}
