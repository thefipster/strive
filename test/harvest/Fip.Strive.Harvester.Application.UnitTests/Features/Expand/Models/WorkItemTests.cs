using AwesomeAssertions;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Harvester.Application.Features.Expand.Models;

namespace Fip.Strive.Harvester.Application.UnitTests.Features.Expand.Models
{
    public class WorkItemTests
    {
        [Fact]
        public void FromSignal_Should_CreateWorkItemWithSignalAndNullFields()
        {
            // Arrange
            var signal = new ImportSignal
            {
                Filepath = "some-file.txt",
                ReferenceId = Guid.NewGuid(),
                EmittedAt = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            };

            // Act
            var workItem = WorkItem.FromSignal(signal);

            // Assert
            workItem.Signal.Should().BeSameAs(signal);
            workItem.OutputPath.Should().BeNull();
            workItem.Hash.Should().BeNull();
        }

        [Fact]
        public void ToSignalWithoutIndex_Should_ThrowInvalidOperationException()
        {
            // Arrange
            var signal = new ImportSignal
            {
                Filepath = "some-file.txt",
                ReferenceId = Guid.NewGuid(),
                EmittedAt = DateTime.UtcNow,
                Id = Guid.NewGuid(),
            };
            var workItem = WorkItem.FromSignal(signal);

            // Act & Assert
            FluentActions
                // ReSharper disable once AccessToDisposedClosure
                .Invoking(() => workItem.ToSignal())
                .Should()
                .Throw<InvalidOperationException>();
        }

        [Fact]
        public void ToIndex_Should_SetHashOnWorkItemAndReturnFileIndexWithExpectedValues()
        {
            // Arrange
            var signal = new ImportSignal
            {
                Filepath = "some-file.txt",
                ReferenceId = Guid.NewGuid(),
                EmittedAt = new DateTime(2021, 6, 15, 12, 30, 0, DateTimeKind.Utc),
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            };
            var workItem = WorkItem.FromSignal(signal);
            var expectedHash = "abc123hash";

            // Act
            var index = workItem.ToIndex(expectedHash);

            // Assert
            workItem.Hash.Should().Be(expectedHash);
            index.Should().NotBeNull();
            index.Hash.Should().Be(expectedHash);
            index.ReferenceId.Should().Be(signal.ReferenceId);
            index.SignalledAt.Should().Be(signal.EmittedAt);
            index.SignalId.Should().Be(signal.Id);
        }
    }
}
