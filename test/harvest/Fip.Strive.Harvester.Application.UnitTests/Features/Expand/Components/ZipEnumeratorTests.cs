using AwesomeAssertions;
using Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Harvester.Application.Features.Expand.Component;
using Fip.Strive.Harvester.Application.Features.Expand.Component.Contracts;
using Fip.Strive.Harvester.Application.Features.Expand.Models;
using NSubstitute;

namespace Fip.Strive.Harvester.Application.UnitTests.Features.Expand.Components
{
    public class ZipEnumeratorTests
    {
        [Fact]
        public async Task ExploreFolderAsync_WhenOutputPathIsNull_ThrowsInvalidOperationException()
        {
            // Arrange
            var checker = Substitute.For<IFileHashGate>();
            var directoryService = Substitute.For<IDirectoryService>();

            var enumerator = new ZipEnumerator(checker, directoryService);

            var work = new WorkItem
            {
                Signal = new ImportSignal { Filepath = Path.Combine("C:", "zips", "archive.zip") },
                OutputPath = null,
            };

            // Act & Assert
            await FluentActions
                .Invoking(async () =>
                    await enumerator.ExploreFolderAsync(work, CancellationToken.None)
                )
                .Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("*null output path*");
        }

        [Fact]
        public async Task ExploreFolderAsync_EnumeratesFilesAndChecksEachFile()
        {
            // Arrange
            var checker = Substitute.For<IFileHashGate>();
            var directoryService = Substitute.For<IDirectoryService>();

            var enumerator = new ZipEnumerator(checker, directoryService);

            var outputPath = Path.Combine("C:", "target", "archive");
            var work = new WorkItem
            {
                Signal = new ImportSignal { Filepath = Path.Combine("C:", "zips", "archive.zip") },
                OutputPath = outputPath,
            };
            var index = work.ToIndex("3gheubih9hghergiuh");

            var files = new[]
            {
                Path.Combine(outputPath, "file1.txt"),
                Path.Combine(outputPath, "sub", "file2.txt"),
            };

            directoryService.EnumerateAllFiles(outputPath).Returns(files);
            checker
                .CheckFileAsync(
                    Arg.Any<WorkItem>(),
                    Arg.Any<string>(),
                    Arg.Any<CancellationToken>()
                )
                .Returns(Task.FromResult(index));

            // Act
            await enumerator.ExploreFolderAsync(work, CancellationToken.None);

            // Assert
            directoryService.Received(1).EnumerateAllFiles(outputPath);
            checker.Received(1).CheckFileAsync(work, files[0], CancellationToken.None);
            checker.Received(1).CheckFileAsync(work, files[1], CancellationToken.None);
        }

        [Fact]
        public async Task ExploreFolderAsync_ThrowsOperationCanceledWhenCancellationRequested()
        {
            // Arrange
            var checker = Substitute.For<IFileHashGate>();
            var directoryService = Substitute.For<IDirectoryService>();

            var enumerator = new ZipEnumerator(checker, directoryService);

            var outputPath = Path.Combine("C:", "target", "archive");
            var work = new WorkItem
            {
                Signal = new ImportSignal { Filepath = Path.Combine("C:", "zips", "archive.zip") },
                OutputPath = outputPath,
            };

            var files = new[] { Path.Combine(outputPath, "file1.txt") };
            directoryService.EnumerateAllFiles(outputPath).Returns(files);

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act
            await FluentActions
                .Invoking(async () => await enumerator.ExploreFolderAsync(work, cts.Token))
                .Should()
                .ThrowAsync<OperationCanceledException>();

            // Assert
            checker.DidNotReceiveWithAnyArgs().CheckFileAsync(default, default, default);
        }
    }
}
