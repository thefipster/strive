using Fip.Strive.Harvester.Application.Defaults;

namespace Fip.Strive.Harvester.Application.Core.PubSub.Contracts;

public interface IPubSubClient
{
    Task SubscribeAsync(
        DirectExchange exchange,
        DirectExchange quarantine,
        Func<string, CancellationToken, Task> processor,
        CancellationToken ct
    );

    Task PublishAsync(string message, DirectExchange exchange);

    ValueTask DisposeAsync();
}
