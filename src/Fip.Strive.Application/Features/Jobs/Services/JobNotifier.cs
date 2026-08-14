using Fip.Strive.Application.Features.Jobs.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace Fip.Strive.Application.Features.Jobs.Services;

public sealed class JobNotifier(ILogger<JobNotifier> logger) : IJobNotifier
{
    private readonly Lock _gate = new();
    private readonly List<Action> _handlers = [];

    public void Notify()
    {
        Action[] handlers;

        lock (_gate)
        {
            handlers = [.. _handlers];
        }

        foreach (var handler in handlers)
        {
            try
            {
                handler();
            }
            catch (Exception exception)
            {
                // A circuit that went away mid-notification is ordinary. The runner must not care.
                logger.LogDebug(exception, "A job subscriber faulted and was skipped");
            }
        }
    }

    public IDisposable Subscribe(Action handler)
    {
        lock (_gate)
        {
            _handlers.Add(handler);
        }

        return new Subscription(this, handler);
    }

    private void Unsubscribe(Action handler)
    {
        lock (_gate)
        {
            _handlers.Remove(handler);
        }
    }

    private sealed class Subscription(JobNotifier notifier, Action handler) : IDisposable
    {
        public void Dispose() => notifier.Unsubscribe(handler);
    }
}
