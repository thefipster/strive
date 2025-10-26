using System.Reflection;
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
    [Fact]
    public async Task CheckFileAsync_WhenSingleFile_EnqueuesSignal()
    {
        // Arrange
        var fileHash = "FER34teEG43g34g";
        var index = new FileIndex { Hash = fileHash };
        index.AddFile("only-one.txt");
        index.Files.Should().NotBeNull("test setup requires a non-null Files collection");

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

        var filepath = @"C:\some\path.txt";
        var work = new WorkItem { Signal = new ImportSignal { Filepath = filepath } };

        // Act
        var result = await decorator.CheckFileAsync(work, filepath, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(index);
        queue.Received(1).EnqueueAsync(Arg.Any<Signal>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CheckFileAsync_WhenMultipleFiles_DoesNotEnqueue()
    {
        // Arrange
        var fileHash = "FER34teEG43g34g";
        var index = new FileIndex { Hash = fileHash };
        index.AddFile("one.txt");
        index.AddFile("two.txt");
        index.Files.Should().HaveCountGreaterThan(1, "test setup requires multiple files");

        var component = Substitute.For<IFileHashGate>();
        component
            .CheckFileAsync(Arg.Any<WorkItem>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(index));

        var queue = Substitute.For<ISignalQueue>();
        var logger = Substitute.For<ILogger<FileHashGateSignalled>>();

        var decorator = new FileHashGateSignalled(component, queue, logger);

        var filepath = @"C:\some\path.txt";
        var work = new WorkItem { Signal = new ImportSignal { Filepath = filepath } };

        // Act
        var result = await decorator.CheckFileAsync(work, filepath, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(index);
        queue.DidNotReceive().EnqueueAsync(Arg.Any<Signal>(), Arg.Any<CancellationToken>());
    }
}
