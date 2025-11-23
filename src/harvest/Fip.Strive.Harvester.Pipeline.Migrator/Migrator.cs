using Fip.Strive.Harvester.Domain.Defaults;
using RabbitMQ.Client;

namespace Fip.Strive.Harvester.Pipeline.Migrator;

public class Migrator(IConnectionFactory factory)
{
    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        var connection = await factory.CreateConnectionAsync(ct);
        var channel = await connection.CreateChannelAsync(cancellationToken: ct);

        await EnsureDirectExchange(HarvestPipelineExchange.Quarantine, channel, ct);

        foreach (var exchange in HarvestPipelineExchange.All)
            await EnsureDirectExchange(exchange, channel, ct);
    }

    private async Task EnsureDirectExchange(
        DirectExchange exchange,
        IChannel channel,
        CancellationToken ct
    )
    {
        await channel.ExchangeDeclareAsync(
            exchange.Exchange,
            ExchangeType.Direct,
            durable: true,
            cancellationToken: ct
        );

        await channel.QueueDeclareAsync(
            exchange.Queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: ct
        );

        await channel.QueueBindAsync(
            exchange.Queue,
            exchange.Exchange,
            exchange.Route,
            cancellationToken: ct
        );
    }
}
