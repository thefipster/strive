using Fip.Strive.Harvester.Application.Defaults;

namespace Fip.Strive.Harvester.Application.Core.PubSub.Contracts;

public interface ISubClient : IAsyncDisposable
{
    event Func<string, CancellationToken, Task>? MessageReceived;
    Task SubscribeAsync(DirectExchange exchange, CancellationToken ct);
}
