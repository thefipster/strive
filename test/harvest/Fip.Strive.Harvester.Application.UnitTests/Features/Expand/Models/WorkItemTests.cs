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
                Hash = "hash1234",
            };

            // Act
            var workItem = WorkItem.FromSignal(signal);

            // Assert
            workItem.Signal.Should().BeSameAs(signal);
            workItem.OutputPath.Should().BeNull();
        }
    }
}
