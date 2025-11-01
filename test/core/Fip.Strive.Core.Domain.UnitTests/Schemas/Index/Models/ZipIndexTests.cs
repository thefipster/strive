using AwesomeAssertions;
using Fip.Strive.Core.Domain.Schemas.Index.Models;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;

namespace Fip.Strive.Core.Domain.UnitTests.Schemas.Index.Models
{
    public class ZipIndexTests
    {
        [Fact]
        public void DefaultInitialization_Should_HaveEmptyFilesAndRecentCreatedAt()
        {
            // Arrange & Act
            var index = new ZipIndex { Hash = "hash-1" };

            // Assert
            index.Files.Should().NotBeNull();
            index.Files.Should().BeEmpty();
            index.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            index.SignalledAt.Should().Be(default(DateTime));
            index.SignalId.Should().Be(Guid.Empty);
            index.ReferenceId.Should().Be(Guid.Empty);
            index.Hash.Should().Be("hash-1");
        }

        [Fact]
        public void AddFile_WhenNew_Should_AddEntryWithRecentTimestamp()
        {
            // Arrange
            var index = new ZipIndex { Hash = "hash-2" };
            var filename = "folder/file1.txt";

            // Act
            index.AddFile(filename);

            // Assert
            index.Files.Should().ContainKey(filename);
            index.Files[filename].Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void AddFile_WhenAlreadyExists_Should_ThrowArgumentExceptionAndPreserveOriginalTimestamp()
        {
            // Arrange
            var index = new ZipIndex { Hash = "hash-3" };
            var filename = "existing.txt";
            var originalTimestamp = DateTime.UtcNow.AddMinutes(-10);
            index.Files.Add(filename, originalTimestamp);

            // Act
            Action act = () => index.AddFile(filename);

            // Assert
            act.Should().Throw<ArgumentException>();
            index.Files.Should().ContainKey(filename);
            index.Files[filename].Should().Be(originalTimestamp);
        }

        [Fact]
        public void From_Should_CreateZipIndexFromSignalAndHash()
        {
            // Arrange
            var emitted = new DateTime(2022, 5, 10, 8, 30, 0, DateTimeKind.Utc);
            var signal = new ImportSignal
            {
                Filepath = "some-file.zip",
                ReferenceId = Guid.NewGuid(),
                EmittedAt = emitted,
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Hash = "hash1234",
            };
            var hash = "zip-hash-abc";

            // Act
            var zipIndex = new ZipIndex
            {
                Hash = hash,
                SignalId = signal.Id,
                ReferenceId = signal.ReferenceId,
                SignalledAt = signal.EmittedAt,
            };

            // Assert
            zipIndex.Should().NotBeNull();
            zipIndex.Hash.Should().Be(hash);
            zipIndex.SignalId.Should().Be(signal.Id);
            zipIndex.ReferenceId.Should().Be(signal.ReferenceId);
            zipIndex.SignalledAt.Should().Be(signal.EmittedAt);
            zipIndex.Files.Should().BeEmpty();
        }
    }
}
