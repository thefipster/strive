using AwesomeAssertions;
using Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Harvester.Application.Features.Expand;
using Fip.Strive.Harvester.Application.Features.Expand.Component;
using Fip.Strive.Harvester.Application.Features.Expand.Models;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Fip.Strive.Harvester.Application.UnitTests.Features.Expand.Components;

public class UnzipCoordinatorTests
{
    [Fact]
    public void Expand_WhenSuccess_CreatesDirectoryAndExtractsAndSetsOutputPath()
    {
        // Arrange
        var targetRoot = Path.Combine("C:", "target-root");
        var options = Options.Create(new ExpandConfig { Path = targetRoot });

        var zipExtractor = Substitute.For<IZipExtractor>();
        var directoryService = Substitute.For<IDirectoryService>();

        var unzipper = new UnzipCoordinator(options, zipExtractor, directoryService);

        var zipPath = Path.Combine("C:", "zips", "archive.zip");
        var work = new WorkItem
        {
            Signal = new ImportSignal { Filepath = zipPath, Hash = "hash1234" },
        };

        var expectedDirName = Path.GetFileNameWithoutExtension(zipPath);
        var expectedOutputPath = Path.Combine(targetRoot, expectedDirName);

        // Act
        var result = unzipper.Expand(work, overwrite: true);

        // Assert
        directoryService.Received(1).CreateDirectory(targetRoot);
        zipExtractor.Received(1).ExtractToDirectory(zipPath, expectedOutputPath, true);
        result.OutputPath.Should().Be(expectedOutputPath);
    }

    [Fact]
    public void Expand_WhenDirectoryCreationFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var targetRoot = Path.Combine("temp", "target-root");
        var options = Options.Create(new ExpandConfig { Path = targetRoot });

        var zipExtractor = Substitute.For<IZipExtractor>();
        var directoryService = Substitute.For<IDirectoryService>();
        directoryService
            .When(d => d.CreateDirectory(Arg.Any<string>()))
            .Do(_ => throw new Exception("create-failed"));

        var unzipper = new UnzipCoordinator(options, zipExtractor, directoryService);

        var zipPath = Path.Combine("temp", "zips", "archive.zip");
        var work = new WorkItem
        {
            Signal = new ImportSignal { Filepath = zipPath, Hash = "hash1234" },
        };

        // Act
        Action act = () => unzipper.Expand(work);

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*could not be created*");
    }

    [Fact]
    public void Expand_WhenExtractionFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var targetRoot = Path.Combine("temp", "target-root");
        var options = Options.Create(new ExpandConfig { Path = targetRoot });

        var zipExtractor = Substitute.For<IZipExtractor>();
        var directoryService = Substitute.For<IDirectoryService>();

        var zipPath = Path.Combine("temp", "zips", "archive.zip");
        var work = new WorkItem
        {
            Signal = new ImportSignal { Filepath = zipPath, Hash = "hash1234" },
        };

        var expectedOutputPath = Path.Combine(
            targetRoot,
            Path.GetFileNameWithoutExtension(zipPath)
        );

        // make extraction throw
        zipExtractor
            .When(z => z.ExtractToDirectory(zipPath, expectedOutputPath, Arg.Any<bool>()))
            .Do(_ => throw new Exception("extract-failed"));

        var unzipper = new UnzipCoordinator(options, zipExtractor, directoryService);

        // Act
        Action act = () => unzipper.Expand(work, overwrite: false);

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*could not be expanded*");
    }
}
