// ReSharper disable AccessToDisposedClosure

using AwesomeAssertions;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Harvester.Application.Features.Expand.Component.Contracts;
using Fip.Strive.Harvester.Application.Features.Expand.Models;
using Fip.Strive.Harvester.Application.Features.Expand.Services;
using NSubstitute;

namespace Fip.Strive.Harvester.Application.UnitTests.Features.Expand.Services
{
    public class ExpansionServiceTests
    {
        [Fact]
        public async Task UnpackZipFileAsync_WhenExpandSucceeds_CallsScannerWithReturnedWorkItem()
        {
            // Arrange
            var zipExtractor = Substitute.For<IZipExtractor>();
            var scanner = Substitute.For<IZipEnumerator>();
            var service = new ExpansionService(zipExtractor, scanner);

            var signal = new ImportSignal
            {
                ReferenceId = Guid.NewGuid(),
                Filepath = @"C:\zips\archive.zip",
                Hash = "hash1234",
            };

            var expectedWork = WorkItem.FromSignal(signal);
            expectedWork.OutputPath = @"C:\target\archive";

            zipExtractor
                .Expand(Arg.Any<WorkItem>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(expectedWork);

            var ct = CancellationToken.None;

            // Act
            await service.UnpackZipFileAsync(signal, ct);

            // Assert
            zipExtractor.Received(1).Expand(Arg.Is<WorkItem>(w => w.Signal == signal), false, ct);
            await scanner.Received(1).ExploreFolderAsync(expectedWork, ct);
        }

        [Fact]
        public async Task UnpackZipFileAsync_WhenExpandThrowsOperationCanceled_ExceptionBubblesAndScannerNotCalled()
        {
            // Arrange
            var zipExtractor = Substitute.For<IZipExtractor>();
            var scanner = Substitute.For<IZipEnumerator>();
            var service = new ExpansionService(zipExtractor, scanner);

            var signal = new ImportSignal
            {
                ReferenceId = Guid.NewGuid(),
                Filepath = @"C:\zips\archive.zip",
                Hash = "hash1234",
            };

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            zipExtractor
                .When(x =>
                    x.Expand(Arg.Any<WorkItem>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                )
                .Do(ci => throw new OperationCanceledException(cts.Token));

            // Act
            Func<Task> act = async () => await service.UnpackZipFileAsync(signal, cts.Token);

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            await scanner.DidNotReceiveWithAnyArgs().ExploreFolderAsync(default, default);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }
    }
}
