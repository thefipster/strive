using AwesomeAssertions;
using Fip.Strive.Core.Domain.Schemas.Queue.Enums;
using Fip.Strive.Core.Domain.Schemas.Queue.Models;

namespace Fip.Strive.Core.Domain.UnitTests.Schemas.Queue.Models
{
    public class JobDetailsTests
    {
        [Fact]
        public void DefaultConstructor_Should_InitializeDefaults()
        {
            // Act
            var job = new JobDetails();

            // Assert
            job.Id.Should().Be(Guid.Empty);
            job.Type.Should().Be(default(SignalTypes));
            job.Payload.Should().BeNull();
            job.Error.Should().BeNull();
            job.Status.Should().Be(JobStatus.Stored);
            job.StartedAt.Should().BeNull();
            job.FinishedAt.Should().BeNull();
            job.SignalledAt.Should().Be(default(DateTime));
            job.CreatedAt.Should().NotBe(default(DateTime));
        }

        [Fact]
        public void Properties_WhenAssigned_Should_PersistValues()
        {
            // Arrange
            var job = new JobDetails();
            var id = Guid.NewGuid();
            var created = new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc);
            var started = created.AddMinutes(1);
            var finished = created.AddMinutes(2);

            // Act
            job.Id = id;
            job.Type = (SignalTypes)7;
            job.Payload = "payload-data";
            job.Error = "some error";
            job.Status = (JobStatus)99;
            job.CreatedAt = created;
            job.StartedAt = started;
            job.FinishedAt = finished;
            job.SignalledAt = created.AddDays(-1);

            // Assert
            job.Id.Should().Be(id);
            job.Type.Should().Be((SignalTypes)7);
            job.Payload.Should().Be("payload-data");
            job.Error.Should().Be("some error");
            job.Status.Should().Be((JobStatus)99);
            job.CreatedAt.Should().Be(created);
            job.StartedAt.Should().Be(started);
            job.FinishedAt.Should().Be(finished);
            job.SignalledAt.Should().Be(created.AddDays(-1));
        }

        [Fact]
        public void CreatedAt_DefaultValue_ShouldBeCloseToUtcNow()
        {
            // Act
            var job = new JobDetails();

            // Assert
            job.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }
    }
}
