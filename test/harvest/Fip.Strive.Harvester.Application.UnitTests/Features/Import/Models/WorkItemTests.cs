using AwesomeAssertions;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Harvester.Application.Features.Import.Models;

namespace Fip.Strive.Harvester.Application.UnitTests.Features.Import.Models;

public class WorkItemTests
{
    [Fact]
    public void FromSignal_ShouldCreateWorkItemWithSignalAndDefaultValues()
    {
        // Arrange
        var uploadSignal = UploadSignal.From("files/upload/test.zip");

        // Act
        var workItem = WorkItem.FromSignal(uploadSignal);

        // Assert
        workItem.Signal.Should().BeSameAs(uploadSignal);
        workItem.ImportedPath.Should().BeNull();
        workItem.Hash.Should().BeNull();
        workItem.Index.Should().BeNull();
        workItem.Skip.Should().BeFalse();
    }

    [Fact]
    public void Filename_ShouldExtractFilenameFromSignalFilepath()
    {
        // Arrange
        var uploadSignal = UploadSignal.From("files/upload/folder/myarchive.zip");
        var workItem = WorkItem.FromSignal(uploadSignal);

        // Act
        var filename = workItem.Filename;

        // Assert
        filename.Should().Be("myarchive.zip");
    }

    [Fact]
    public void Filename_WithDifferentPathSeparators_ShouldExtractCorrectly()
    {
        // Arrange
        var uploadSignal = UploadSignal.From("files/upload/folder/file.zip");
        var workItem = WorkItem.FromSignal(uploadSignal);

        // Act
        var filename = workItem.Filename;

        // Assert
        filename.Should().Be("file.zip");
    }

    [Fact]
    public void ToSignal_ShouldCreateImportSignalWithReferenceIdAndFilepath()
    {
        // Arrange
        var uploadSignal = UploadSignal.From("files/upload/test.zip");
        var workItem = WorkItem.FromSignal(uploadSignal);
        var filepath = @"C:\import\test.zip";

        // Act
        var importSignal = workItem.ToSignal(filepath);

        // Assert
        importSignal.Should().NotBeNull();
        importSignal.ReferenceId.Should().Be(uploadSignal.ReferenceId);
        importSignal.Filepath.Should().Be(filepath);
    }

    [Fact]
    public void ToIndex_WhenHashIsSet_ShouldCreateZipIndexWithCorrectProperties()
    {
        // Arrange
        var uploadSignal = UploadSignal.From("files/upload/archive.zip");
        var workItem = WorkItem.FromSignal(uploadSignal);
        workItem.Hash = "abc123hash";

        // Act
        var index = workItem.ToIndex();

        // Assert
        index.Should().NotBeNull();
        index.Hash.Should().Be("abc123hash");
        index.SignalId.Should().Be(uploadSignal.Id);
        index.ReferenceId.Should().Be(uploadSignal.ReferenceId);
        index.SignalledAt.Should().Be(uploadSignal.EmittedAt);
        index.Files.Should().ContainKey("archive.zip");
    }

    [Fact]
    public void ToIndex_WhenHashIsNull_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var uploadSignal = UploadSignal.From("files/upload/test.zip");
        var workItem = WorkItem.FromSignal(uploadSignal);
        workItem.Hash = null;

        // Act
        var act = () => workItem.ToIndex();

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Cannot create ZipIndex when hash is null or empty.");
    }

    [Fact]
    public void ToIndex_WhenHashIsEmpty_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var uploadSignal = UploadSignal.From("files/upload/test.zip");
        var workItem = WorkItem.FromSignal(uploadSignal);
        workItem.Hash = string.Empty;

        // Act
        var act = () => workItem.ToIndex();

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Cannot create ZipIndex when hash is null or empty.");
    }

    [Fact]
    public void ToIndex_WhenHashIsWhitespace_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var uploadSignal = UploadSignal.From("files/upload/test.zip");
        var workItem = WorkItem.FromSignal(uploadSignal);
        workItem.Hash = "   ";

        // Act
        var act = () => workItem.ToIndex();

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Cannot create ZipIndex when hash is null or empty.");
    }

    [Fact]
    public void ToIndex_ShouldAddFilenameToIndexFiles()
    {
        // Arrange
        var uploadSignal = UploadSignal.From("files/upload/subfolder/myfile.zip");
        var workItem = WorkItem.FromSignal(uploadSignal);
        workItem.Hash = "hash123";

        // Act
        var index = workItem.ToIndex();

        // Assert
        index.Files.Should().HaveCount(1);
        index.Files.Should().ContainKey("myfile.zip");
    }

    [Fact]
    public void Skip_DefaultValue_ShouldBeFalse()
    {
        // Arrange
        var uploadSignal = UploadSignal.From("files/upload/test.zip");

        // Act
        var workItem = WorkItem.FromSignal(uploadSignal);

        // Assert
        workItem.Skip.Should().BeFalse();
    }

    [Fact]
    public void Skip_WhenSet_ShouldPersistValue()
    {
        // Arrange
        var uploadSignal = UploadSignal.From("files/upload/test.zip");
        var workItem = WorkItem.FromSignal(uploadSignal);

        // Act
        workItem.Skip = true;

        // Assert
        workItem.Skip.Should().BeTrue();
    }

    [Fact]
    public void ImportedPath_WhenSet_ShouldPersistValue()
    {
        // Arrange
        var uploadSignal = UploadSignal.From("files/upload/test.zip");
        var workItem = WorkItem.FromSignal(uploadSignal);
        var importedPath = "files/import/test.zip";

        // Act
        workItem.ImportedPath = importedPath;

        // Assert
        workItem.ImportedPath.Should().Be(importedPath);
    }

    [Fact]
    public void Hash_WhenSet_ShouldPersistValue()
    {
        // Arrange
        var uploadSignal = UploadSignal.From("files/upload/test.zip");
        var workItem = WorkItem.FromSignal(uploadSignal);
        var hash = "myhash123";

        // Act
        workItem.Hash = hash;

        // Assert
        workItem.Hash.Should().Be(hash);
    }

    [Fact]
    public void ToSignal_ShouldInheritReferenceIdFromUploadSignal()
    {
        // Arrange
        var uploadSignal = UploadSignal.From("files/upload/test.zip");
        var expectedReferenceId = uploadSignal.ReferenceId;
        var workItem = WorkItem.FromSignal(uploadSignal);

        // Act
        var importSignal = workItem.ToSignal("files/import/test.zip");

        // Assert
        importSignal.ReferenceId.Should().Be(expectedReferenceId);
    }

    [Fact]
    public void ToIndex_WithComplexFilepath_ShouldExtractFilenameCorrectly()
    {
        // Arrange
        var uploadSignal = UploadSignal.From(@"very/long/path/with/many/folders/archive.zip");
        var workItem = WorkItem.FromSignal(uploadSignal);
        workItem.Hash = "complexhash";

        // Act
        var index = workItem.ToIndex();

        // Assert
        index.Files.Should().ContainKey("archive.zip");
    }

    [Fact]
    public void WorkItem_AllProperties_ShouldBeSettableAndGettable()
    {
        // Arrange
        var uploadSignal = UploadSignal.From("files/upload/test.zip");
        var workItem = WorkItem.FromSignal(uploadSignal);
        var importedPath = "files/import/test.zip";
        var hash = "testhash";

        // Act
        workItem.ImportedPath = importedPath;
        workItem.Hash = hash;
        workItem.Skip = true;

        // Assert
        workItem.Signal.Should().BeSameAs(uploadSignal);
        workItem.ImportedPath.Should().Be(importedPath);
        workItem.Hash.Should().Be(hash);
        workItem.Skip.Should().BeTrue();
        workItem.Filename.Should().Be("test.zip");
    }
}
