using AwesomeAssertions;
using Fip.Strive.Harvester.Application.Core.Queue;
using Fip.Strive.Harvester.Application.Core.Queue.Components;
using Fip.Strive.Harvester.Application.Core.Queue.Enums;
using Fip.Strive.Harvester.Application.Core.Queue.Models;
using Fip.Strive.Harvester.Application.Core.Queue.Repositories.Contracts;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Fip.Strive.Harvester.Application.UnitTests.Core.Queue.Components;

public class LiteDbSignalQueueTests
{
    [Fact]
    public async Task EnqueueAsync_WhenQueueNotFull_AddsJobToPendingQueue()
    {
        // Arrange
        var jobControl = Substitute.For<IJobControl>();
        var config = Options.Create(new QueueConfig
        {
            QueueCountLimit = 100,
            QueueBatchSize = 10
        });
        var queue = new LiteDbSignalQueue(jobControl, config);
        var signal = new Signal(SignalTypes.VoidSignal);

        // Act
        await queue.EnqueueAsync(signal);

        // Assert
        queue.Count.Should().Be(1);
        jobControl.Received(1).Insert(Arg.Any<JobDetails>());
    }

    [Fact]
    public async Task DequeueAsync_WhenQueueHasItems_ReturnsJobAndMarksAsStarted()
    {
        // Arrange
        var jobControl = Substitute.For<IJobControl>();
        var config = Options.Create(new QueueConfig
        {
            QueueCountLimit = 100,
            QueueBatchSize = 10
        });
        var queue = new LiteDbSignalQueue(jobControl, config);
        var signal = new Signal(SignalTypes.VoidSignal);

        // Act
        await queue.EnqueueAsync(signal);
        var result = await queue.DequeueAsync();

        // Assert
        result.Should().NotBeNull();
        queue.Count.Should().Be(0);
        jobControl.Received(1).MarkAsStarted(Arg.Any<Guid>());
    }

    [Fact]
    public async Task DequeueAsync_WhenQueueNeedsRefill_LoadsJobsFromStorage()
    {
        // Arrange
        var jobControl = Substitute.For<IJobControl>();
        var config = Options.Create(new QueueConfig
        {
            QueueCountLimit = 100,
            QueueBatchSize = 10
        });
        
        var storedJobs = new List<JobDetails>
        {
            new JobDetails { Id = Guid.NewGuid(), Type = SignalTypes.VoidSignal, Status = JobStatus.Stored },
            new JobDetails { Id = Guid.NewGuid(), Type = SignalTypes.VoidSignal, Status = JobStatus.Stored }
        };
        
        jobControl.GetStored(10).Returns(storedJobs);
        
        var queue = new LiteDbSignalQueue(jobControl, config);

        // Act
        var result = await queue.DequeueAsync();

        // Assert
        jobControl.Received(1).GetStored(10);
        queue.Count.Should().Be(2);
    }
}