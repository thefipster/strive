using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace Fip.Strive.Harvester.Application.Core.Queue.Models;

public class QueueMetrics(IOptions<QueueConfig> config)
{
    private readonly TimeSpan _windowSize = TimeSpan.FromSeconds(config.Value.RateWindowSeconds);
    private readonly ConcurrentQueue<DateTime> _completionTimes = new();
    private int _activeWorkers;

    public int ActiveWorkers => _activeWorkers;

    public void IncrementActiveWorkers() => Interlocked.Increment(ref _activeWorkers);

    public void DecrementActiveWorkers() => Interlocked.Decrement(ref _activeWorkers);

    public void RecordCompletion()
    {
        var now = DateTime.UtcNow;
        _completionTimes.Enqueue(now);
        while (_completionTimes.TryPeek(out var oldest) && now - oldest > _windowSize)
            _completionTimes.TryDequeue(out _);
    }

    public double GetProcessingRate()
    {
        var now = DateTime.UtcNow;
        while (_completionTimes.TryPeek(out var oldest) && now - oldest > _windowSize)
            _completionTimes.TryDequeue(out _);

        var rate = _completionTimes.Count / _windowSize.TotalSeconds;
        return Math.Round(rate, 1);
    }
}
