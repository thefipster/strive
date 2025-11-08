using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Queue.Domain.Models;

public class QueueMetrics(IOptions<QueueConfig> config)
{
    private readonly TimeSpan _windowSize = TimeSpan.FromSeconds(config.Value.RateWindowSeconds);
    private readonly ConcurrentQueue<DateTime> _completionTimes = new();
    private readonly ConcurrentQueue<DateTime> _enqueueTimes = new();
    private int _activeWorkers;
    private int _jobs;

    public int ActiveWorkers => _activeWorkers;
    public int Jobs => _jobs;

    public void SetJobCount(int count) => _jobs = count;

    public void IncrementActiveWorkers() => Interlocked.Increment(ref _activeWorkers);

    public void DecrementActiveWorkers() => Interlocked.Decrement(ref _activeWorkers);

    public int GetJobCount() => _jobs;

    public void RecordEnqueue() => Record(_enqueueTimes);

    public void RecordCompletion() => Record(_completionTimes);

    public void Record(ConcurrentQueue<DateTime> queue)
    {
        var now = DateTime.UtcNow;
        queue.Enqueue(now);
        CleanupQueue(queue);
    }

    public double GetProcessingRate() => GetRate(_completionTimes);

    public double GetEnqueueRate() => GetRate(_enqueueTimes);

    public double GetRate(ConcurrentQueue<DateTime> queue)
    {
        CleanupQueue(queue);
        var rate = queue.Count / _windowSize.TotalSeconds;
        return Math.Round(rate, 1);
    }

    public void CleanupQueue(ConcurrentQueue<DateTime> queue)
    {
        var now = DateTime.UtcNow;
        while (queue.TryPeek(out var oldest) && now - oldest > _windowSize)
            queue.TryDequeue(out _);
    }
}
