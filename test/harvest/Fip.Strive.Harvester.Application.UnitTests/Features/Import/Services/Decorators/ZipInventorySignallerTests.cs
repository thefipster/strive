using AwesomeAssertions;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;
using Fip.Strive.Harvester.Application.Features.Import;
using Fip.Strive.Harvester.Application.Features.Import.Models;
using Fip.Strive.Harvester.Application.Features.Import.Services.Contracts;
using Fip.Strive.Harvester.Application.Features.Import.Services.Decorators;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Fip.Strive.Harvester.Application.UnitTests.Features.Import.Services.Decorators;

public class ZipInventorySignallerTests
{
    private readonly IZipInventory _component;
    private readonly ISignalQueue _queue;
    private readonly ILogger<ZipInventorySignaller> _logger;
    private readonly ZipInventorySignaller _sut;
    private readonly string _rootPath;

    public ZipInventorySignallerTests()
    {
        _component = Substitute.For<IZipInventory>();
        _queue = Substitute.For<ISignalQueue>();
        _logger = Substitute.For<ILogger<ZipInventorySignaller>>();
        _rootPath = @"C:\import";
        var config = Options.Create(new ImportConfig { Path = _rootPath });
        _sut = new ZipInventorySignaller(_component, _queue, config, _logger);
    }

    [Fact]
    public async Task ImportAsync_WhenWorkSkipIsFalse_ShouldEnqueueSignalAndReturnWork()
    {
        // Arrange
        var uploadSignal = UploadSignal.From(@"C:\upload\test.zip", "hash1234");
        var work = WorkItem.FromSignal(uploadSignal);
        work.ImportedPath = "test.zip";
        work.Skip = false;

        _component.ImportAsync(uploadSignal, Arg.Any<CancellationToken>()).Returns(work);

        // Act
        var result = await _sut.ImportAsync(uploadSignal, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(work);
        await _queue
            .Received(1)
            .EnqueueAsync(
                Arg.Is<ImportSignal>(s =>
                    s.ReferenceId == uploadSignal.ReferenceId
                    && s.Filepath == Path.Combine(_rootPath, "test.zip")
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task ImportAsync_WhenWorkSkipIsTrue_ShouldNotEnqueueSignalAndLogInformation()
    {
        // Arrange
        var uploadSignal = UploadSignal.From(@"C:\upload\duplicate.zip", "hash1234");
        var work = WorkItem.FromSignal(uploadSignal);
        work.ImportedPath = "duplicate.zip";
        work.Skip = true;

        _component.ImportAsync(uploadSignal, Arg.Any<CancellationToken>()).Returns(work);

        // Act
        var result = await _sut.ImportAsync(uploadSignal, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(work);
        await _queue
            .DidNotReceive()
            .EnqueueAsync(Arg.Any<ImportSignal>(), Arg.Any<CancellationToken>());
        _logger.Received(1);
    }

    [Fact]
    public async Task ImportAsync_ShouldCallComponentImportAsync()
    {
        // Arrange
        var uploadSignal = UploadSignal.From(@"C:\upload\test.zip", "hash1234");
        var work = WorkItem.FromSignal(uploadSignal);
        work.ImportedPath = "test.zip";
        work.Skip = false;
        var cancellationToken = new CancellationToken();

        _component.ImportAsync(uploadSignal, cancellationToken).Returns(work);

        // Act
        await _sut.ImportAsync(uploadSignal, cancellationToken);

        // Assert
        await _component.Received(1).ImportAsync(uploadSignal, cancellationToken);
    }

    [Fact]
    public async Task ImportAsync_WhenEnqueueing_ShouldCreateImportSignalWithCorrectFilepath()
    {
        // Arrange
        var uploadSignal = UploadSignal.From(@"C:\upload\myfile.zip", "hash1234");
        var work = WorkItem.FromSignal(uploadSignal);
        work.ImportedPath = "myfile.zip";
        work.Skip = false;

        _component.ImportAsync(uploadSignal, Arg.Any<CancellationToken>()).Returns(work);

        // Act
        await _sut.ImportAsync(uploadSignal, CancellationToken.None);

        // Assert
        await _queue
            .Received(1)
            .EnqueueAsync(
                Arg.Is<ImportSignal>(s => s.Filepath == Path.Combine(_rootPath, "myfile.zip")),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task ImportAsync_WhenEnqueueing_ShouldInheritReferenceIdFromUploadSignal()
    {
        // Arrange
        var uploadSignal = UploadSignal.From(@"C:\upload\test.zip", "hash1234");
        var expectedReferenceId = uploadSignal.ReferenceId;
        var work = WorkItem.FromSignal(uploadSignal);
        work.ImportedPath = "test.zip";
        work.Skip = false;

        _component.ImportAsync(uploadSignal, Arg.Any<CancellationToken>()).Returns(work);

        // Act
        await _sut.ImportAsync(uploadSignal, CancellationToken.None);

        // Assert
        await _queue
            .Received(1)
            .EnqueueAsync(
                Arg.Is<ImportSignal>(s => s.ReferenceId == expectedReferenceId),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task ImportAsync_WhenEnqueueing_ShouldPassCancellationTokenToQueue()
    {
        // Arrange
        var uploadSignal = UploadSignal.From(@"C:\upload\test.zip", "hash1234");
        var work = WorkItem.FromSignal(uploadSignal);
        work.ImportedPath = "test.zip";
        work.Skip = false;
        var cancellationToken = new CancellationToken();

        _component.ImportAsync(uploadSignal, cancellationToken).Returns(work);

        // Act
        await _sut.ImportAsync(uploadSignal, cancellationToken);

        // Assert
        await _queue.Received(1).EnqueueAsync(Arg.Any<ImportSignal>(), cancellationToken);
    }

    [Fact]
    public async Task ImportAsync_WhenWorkSkipIsTrue_ShouldReturnWorkWithoutModification()
    {
        // Arrange
        var uploadSignal = UploadSignal.From(@"C:\upload\known.zip", "hash1234");
        var work = WorkItem.FromSignal(uploadSignal);
        work.ImportedPath = "known.zip";
        work.Skip = true;

        _component.ImportAsync(uploadSignal, Arg.Any<CancellationToken>()).Returns(work);

        // Act
        var result = await _sut.ImportAsync(uploadSignal, CancellationToken.None);

        // Assert
        result.ImportedPath.Should().Be("known.zip");
        result.Skip.Should().BeTrue();
    }

    [Fact]
    public async Task ImportAsync_WithDifferentRootPath_ShouldUseConfiguredPath()
    {
        // Arrange
        var customRootPath = @"D:\custom\import";
        var customConfig = Options.Create(new ImportConfig { Path = customRootPath });
        var customSut = new ZipInventorySignaller(_component, _queue, customConfig, _logger);

        var uploadSignal = UploadSignal.From(@"C:\upload\test.zip", "hash1234");
        var work = WorkItem.FromSignal(uploadSignal);
        work.ImportedPath = "test.zip";
        work.Skip = false;

        _component.ImportAsync(uploadSignal, Arg.Any<CancellationToken>()).Returns(work);

        // Act
        await customSut.ImportAsync(uploadSignal, CancellationToken.None);

        // Assert
        await _queue
            .Received(1)
            .EnqueueAsync(
                Arg.Is<ImportSignal>(s => s.Filepath == Path.Combine(customRootPath, "test.zip")),
                Arg.Any<CancellationToken>()
            );
    }
}
