using AwesomeAssertions;
using Fip.Strive.Core.Domain.Schemas.Index.Models;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;
using Fip.Strive.Harvester.Application.Features.Expand.Component.Contracts;
using Fip.Strive.Harvester.Application.Features.Expand.Component.Decorators;
using Fip.Strive.Harvester.Application.Features.Expand.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fip.Strive.Harvester.Application.UnitTests.Features.Expand.Components.Decorators;

public class FileHashGateSignalledTests
{
    private readonly IFileHashGate _component;
    private readonly ISignalQueue _queue;
    private readonly ILogger<FileHashGateSignalled> _logger;
    private readonly FileHashGateSignalled _sut;

    public FileHashGateSignalledTests()
    {
        _component = Substitute.For<IFileHashGate>();
        _queue = Substitute.For<ISignalQueue>();
        _logger = Substitute.For<ILogger<FileHashGateSignalled>>();
        _sut = new FileHashGateSignalled(_component, _queue, _logger);
    }

    [Fact]
    public async Task CheckFileAsync_WhenIndexHasOneFile_ShouldEnqueueSignalAndReturnIndex()
    {
        // Arrange
        var hash = "somehash";
        var filepath = @"C:\temp\newfile.txt";
        var referenceId = Guid.NewGuid();
        var signal = new ImportSignal
        {
            Filepath = "archive.zip",
            ReferenceId = referenceId,
            EmittedAt = DateTime.UtcNow,
            Id = Guid.NewGuid(),
        };
        var work = WorkItem.FromSignal(signal);
        var index = work.ToIndex(hash);
        index.AddFile("newfile.txt");

        _component.CheckFileAsync(work, filepath, Arg.Any<CancellationToken>()).Returns(index);

        // Act
        var result = await _sut.CheckFileAsync(work, filepath, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(index);
        await _queue
            .Received(1)
            .EnqueueAsync(
                Arg.Is<FileSignal>(s => s.ReferenceId == referenceId && s.Filepath == hash),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task CheckFileAsync_WhenIndexHasMultipleFiles_ShouldNotEnqueueAndLogInformation()
    {
        // Arrange
        var filepath = @"C:\temp\existingfile.txt";
        var referenceId = Guid.NewGuid();
        var signal = new ImportSignal
        {
            Filepath = "archive.zip",
            ReferenceId = referenceId,
            EmittedAt = DateTime.UtcNow,
            Id = Guid.NewGuid(),
        };
        var work = WorkItem.FromSignal(signal);

        var index = new FileIndex
        {
            Hash = "somehash",
            ReferenceId = referenceId,
            SignalledAt = signal.EmittedAt,
            SignalId = signal.Id,
        };
        index.AddFile("file1.txt");
        index.AddFile("file2.txt");

        _component.CheckFileAsync(work, filepath, Arg.Any<CancellationToken>()).Returns(index);

        // Act
        var result = await _sut.CheckFileAsync(work, filepath, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(index);
        await _queue
            .DidNotReceive()
            .EnqueueAsync(Arg.Any<FileSignal>(), Arg.Any<CancellationToken>());
        _logger.Received(1);
    }

    [Fact]
    public async Task CheckFileAsync_WhenIndexHasNoFiles_ShouldNotEnqueueAndLogError()
    {
        // Arrange
        var filepath = @"C:\temp\errorfile.txt";
        var referenceId = Guid.NewGuid();
        var signal = new ImportSignal
        {
            Filepath = "archive.zip",
            ReferenceId = referenceId,
            EmittedAt = DateTime.UtcNow,
            Id = Guid.NewGuid(),
        };
        var work = WorkItem.FromSignal(signal);

        var index = new FileIndex
        {
            Hash = "somehash",
            ReferenceId = referenceId,
            SignalledAt = signal.EmittedAt,
            SignalId = signal.Id,
        };
        // No files added

        _component.CheckFileAsync(work, filepath, Arg.Any<CancellationToken>()).Returns(index);

        // Act
        var result = await _sut.CheckFileAsync(work, filepath, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(index);
        await _queue
            .DidNotReceive()
            .EnqueueAsync(Arg.Any<FileSignal>(), Arg.Any<CancellationToken>());
        _logger.Received(1);
    }

    [Fact]
    public async Task CheckFileAsync_ShouldCallComponentCheckFileAsync()
    {
        // Arrange
        var filepath = @"C:\temp\testfile.txt";
        var signal = new ImportSignal
        {
            Filepath = "archive.zip",
            ReferenceId = Guid.NewGuid(),
            EmittedAt = DateTime.UtcNow,
            Id = Guid.NewGuid(),
        };

        var work = WorkItem.FromSignal(signal);
        var index = work.ToIndex("r3489t389tu498");
        var cancellationToken = new CancellationToken();

        index.AddFile("testfile.txt");

        _component.CheckFileAsync(work, filepath, cancellationToken).Returns(index);

        // Act
        await _sut.CheckFileAsync(work, filepath, cancellationToken);

        // Assert
        await _component.Received(1).CheckFileAsync(work, filepath, cancellationToken);
    }

    [Fact]
    public async Task CheckFileAsync_WhenEnqueueing_ShouldPassCorrectCancellationToken()
    {
        // Arrange
        var filepath = @"C:\temp\file.txt";
        var signal = new ImportSignal
        {
            Filepath = "archive.zip",
            ReferenceId = Guid.NewGuid(),
            EmittedAt = DateTime.UtcNow,
            Id = Guid.NewGuid(),
        };
        var work = WorkItem.FromSignal(signal);
        var index = work.ToIndex("hash");
        var cancellationToken = new CancellationToken();

        index.AddFile("file.txt");

        _component.CheckFileAsync(work, filepath, cancellationToken).Returns(index);

        // Act
        await _sut.CheckFileAsync(work, filepath, cancellationToken);

        // Assert
        await _queue.Received(1).EnqueueAsync(Arg.Any<FileSignal>(), cancellationToken);
    }
}
