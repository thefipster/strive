using Fip.Strive.Harvester.Domain.Defaults;

namespace Fip.Strive.Harvester.Application.Core.PubSub.Contracts;

public interface IPubSubClient
{
    Task SubscribeAsync(
        DirectExchange exchange,
        Func<string, CancellationToken, Task> processor,
        CancellationToken ct
    );

    Task PublishAsync(string message, DirectExchange exchange);

    ValueTask DisposeAsync();
}
