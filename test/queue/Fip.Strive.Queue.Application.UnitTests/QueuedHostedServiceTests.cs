using AwesomeAssertions;
using Fip.Strive.Queue.Application.UnitTests.Components;
using Fip.Strive.Queue.Application.UnitTests.Fixtures;

namespace Fip.Strive.Queue.Application.UnitTests;

public class QueuedHostedServiceTests(QueueMemoryProviderFixture fixture)
    : IClassFixture<QueueMemoryProviderFixture>
{
    [Fact]
    public async Task EnqueueTestSignal_WithRegisteredTestWorker_ResultsInSucceededJob()
    {
        // Arrange
        var expectedNumber = 42;

        var ops = fixture.Service;
        var queue = fixture.Queue;
        var worker = fixture.Worker;

        _ = Task.Run(() => ops.ExecuteAsync(CancellationToken.None));
        await Task.Delay(100);

        // Act
        await queue.EnqueueAsync(new TestSignal(expectedNumber), CancellationToken.None);
        await Task.Delay(100);

        // Assert
        worker.Number.Should().Be(expectedNumber);
    }
}
