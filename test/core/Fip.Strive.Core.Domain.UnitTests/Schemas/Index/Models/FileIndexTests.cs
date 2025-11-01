using AwesomeAssertions;
using Fip.Strive.Core.Domain.Schemas.Index.Models;

namespace Fip.Strive.Core.Domain.UnitTests.Schemas.Index.Models
{
    public class FileIndexTests
    {
        [Fact]
        public void DefaultInitialization_Should_HaveEmptyFilesAndRecentCreatedAt()
        {
            // Arrange
            var referenceId = Guid.NewGuid();
            var signalId = Guid.NewGuid();

            // Act
            var index = new FileIndex
            {
                Hash = "hash-1",
                ReferenceId = referenceId,
                SignalId = signalId,
                SignalledAt = DateTime.UtcNow,
                ParentId = "hash1234",
            };

            // Assert
            index.Files.Should().NotBeNull();
            index.Files.Should().BeEmpty();
            index.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            index.SignalledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            index.SignalId.Should().Be(signalId);
            index.ReferenceId.Should().Be(referenceId);
        }

        [Fact]
        public void AddFile_WhenNew_Should_AddEntryWithRecentTimestamp()
        {
            // Arrange
            var index = new FileIndex
            {
                Hash = "hash-2",
                ReferenceId = Guid.NewGuid(),
                SignalId = Guid.NewGuid(),
                SignalledAt = DateTime.UtcNow,
                ParentId = "hash1234",
            };
            var filename = "folder/file1.txt";

            // Act
            index.AddFile(filename);

            // Assert
            index.Files.Should().ContainKey(filename);
            index.Files[filename].Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void AddFile_WhenAlreadyExists_Should_NotOverwriteTimestamp()
        {
            // Arrange
            var index = new FileIndex
            {
                Hash = "hash-3",
                ReferenceId = Guid.NewGuid(),
                SignalId = Guid.NewGuid(),
                SignalledAt = DateTime.UtcNow,
                ParentId = "hash1234",
            };
            var filename = "existing.txt";
            var originalTimestamp = DateTime.UtcNow.AddMinutes(-10);
            index.Files.Add(filename, originalTimestamp);

            // Act
            index.AddFile(filename);

            // Assert
            index.Files.Should().ContainKey(filename);
            index.Files.Count.Should().Be(1);
            index.Files[filename].Should().Be(originalTimestamp);
        }
    }
}
