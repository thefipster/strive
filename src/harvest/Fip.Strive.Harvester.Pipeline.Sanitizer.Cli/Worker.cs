using System.Collections.Concurrent;
using System.Diagnostics;
using Fip.Strive.Harvester.Application.Core.PubSub.Contracts;
using Fip.Strive.Harvester.Application.Defaults;
using Fip.Strive.Harvester.Application.Infrastructure.Pipeline;
using Fip.Strive.Harvester.Application.Infrastructure.Pipeline.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fip.Strive.Harvester.Pipeline.Sanitizer.Cli;

public class Worker(ISubClient client, ILogger<Worker> logger, IServiceScopeFactory scopeFactory)
    : BackgroundService
{
    private readonly DirectExchange _quarantine = HarvestPipelineExchange.Quarantine;

    private readonly ConcurrentQueue<string> _buffer = new();
    private readonly SemaphoreSlim _signal = new(0, int.MaxValue);

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        logger.LogInformation("Pipeline sanitizer init.");

        client.MessageReceived += ClientOnMessageReceived;
        await client.SubscribeAsync(_quarantine, ct);

        logger.LogInformation("Pipeline sanitizer start.");

        await ProcessingLoop(ct);

        logger.LogInformation("Pipeline sanitizer exit.");
    }

    private async Task ProcessingLoop(CancellationToken ct)
    {
        // batching loop
        var sw = Stopwatch.StartNew();
        try
        {
            while (!ct.IsCancellationRequested)
            {
                // wait up to 10s for at least one message
                await _signal.WaitAsync(TimeSpan.FromSeconds(10), ct);

                // drain queue
                var batch = new List<string>(capacity: 1024);
                while (_buffer.TryDequeue(out var msg))
                    batch.Add(msg);

                if (batch.Count == 0)
                    continue;

                // write batch
                using var scope = scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<PipelineContext>();

                foreach (var message in batch)
                {
                    var record = QuarantinedSignal.From(message);
                    await context.Quarantined.AddAsync(record, ct);
                }

                await context.SaveChangesAsync(ct);
                logger.LogDebug(
                    "Flushed {Count} quarantined records after {ElapsedMs} ms.",
                    batch.Count,
                    sw.ElapsedMilliseconds
                );

                sw.Restart();
            }
        }
        catch (OperationCanceledException)
        {
            // graceful shutdown: flush remaining messages
            var remaining = new List<string>();
            while (_buffer.TryDequeue(out var msg))
                remaining.Add(msg);

            if (remaining.Count > 0)
            {
                using var scope = scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<PipelineContext>();
                foreach (var message in remaining)
                {
                    var record = QuarantinedSignal.From(message);
                    await context.Quarantined.AddAsync(record, CancellationToken.None);
                }
                await context.SaveChangesAsync(CancellationToken.None);
                logger.LogInformation(
                    "Flushed remaining {Count} quarantined records on shutdown.",
                    remaining.Count
                );
            }
        }
    }

    private Task ClientOnMessageReceived(string message, CancellationToken ct)
    {
        _buffer.Enqueue(message);
        _ = _signal.Release();
        return Task.CompletedTask;
    }
}
