using AwesomeAssertions;
using Fip.Strive.Core.Domain.Schemas.Index.Models;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Harvester.Application.Core.Queue.Components.Contracts;
using Fip.Strive.Harvester.Application.Features.Expand.Component;
using Fip.Strive.Harvester.Application.Features.Expand.Component.Contracts;
using Fip.Strive.Harvester.Application.Features.Expand.Component.Decorators;
using Fip.Strive.Harvester.Application.Features.Expand.Models;
using Fip.Strive.Harvester.Application.Features.Expand.Repositories.Contracts;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fip.Strive.Harvester.Application.UnitTests.Features.Expand.Components;

public class FileHashGateTests
{
    [Fact]
    public async Task CheckFileAsync_WhenIndexExists_AddsFilenameAndUpserts()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, "test-content");
        var fileName = Path.GetFileName(tempFile);
        var fileHash = "FER34teEG43g34g";

        var index = new FileIndex { Hash = fileHash };
        index.AddFile("already-known.txt");

        var indexer = Substitute.For<IFileIndexer>();
        indexer.Find(Arg.Any<string>()).Returns(index);
        indexer.When(x => x.Upsert(Arg.Any<FileIndex>())).Do(_ => { });

        var gate = new FileHashGate(indexer);

        // Act
        var result = await gate.CheckFileAsync(null!, tempFile, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(index);
        index.Files.Should().ContainKey(fileName);
        indexer.Received(1).Upsert(index);

        // Cleanup
        File.Delete(tempFile);
    }
}

public class FileHashGateSignalledTests
{
    [Fact]
    public async Task CheckFileAsync_WhenSingleFile_EnqueuesSignal()
    {
        // Arrange
        var fileHash = "FER34teEG43g34g";
        var index = new FileIndex { Hash = fileHash };
        index.AddFile("only-one.txt"); // single file => should enqueue
        var work = new WorkItem
        {
            Signal = new ImportSignal { Filepath = index.Files.First().Key },
        };

        var component = Substitute.For<IFileHashGate>();
        component
            .CheckFileAsync(Arg.Any<WorkItem>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(index));

        var queue = Substitute.For<ISignalQueue>();
        queue
            .EnqueueAsync(Arg.Any<Signal>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var logger = Substitute.For<ILogger<FileHashGateSignalled>>();

        var decorator = new FileHashGateSignalled(component, queue, logger);

        // Act
        await decorator.CheckFileAsync(work, "C:\\some\\path.txt", CancellationToken.None);

        // Assert
        queue.Received(1).EnqueueAsync(Arg.Any<Signal>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CheckFileAsync_WhenMultipleFiles_DoesNotEnqueue()
    {
        // Arrange
        var fileHash = "FER34teEG43g34g";
        var index = new FileIndex { Hash = fileHash };
        index.AddFile("one.txt");
        index.AddFile("two.txt"); // multiple files => should not enqueue
        var work = new WorkItem
        {
            Signal = new ImportSignal { Filepath = index.Files.First().Key },
        };

        var component = Substitute.For<IFileHashGate>();
        component
            .CheckFileAsync(Arg.Any<WorkItem>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(index));

        var queue = Substitute.For<ISignalQueue>();
        var logger = Substitute.For<ILogger<FileHashGateSignalled>>();

        var decorator = new FileHashGateSignalled(component, queue, logger);

        // Act
        await decorator.CheckFileAsync(work, "C:\\some\\path.txt", CancellationToken.None);

        // Assert
        queue.DidNotReceive().EnqueueAsync(Arg.Any<Signal>(), Arg.Any<CancellationToken>());
    }
}
