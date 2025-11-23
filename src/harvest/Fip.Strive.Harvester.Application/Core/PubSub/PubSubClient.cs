using System.Text;
using Fip.Strive.Harvester.Application.Core.PubSub.Contracts;
using Fip.Strive.Harvester.Domain.Defaults;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Fip.Strive.Harvester.Application.Core.PubSub;

public class PubSubClient(IConnectionFactory factory, ILogger<PubSubClient> logger) : IPubSubClient
{
    private IConnection? _connection;
    private IChannel? _channel;

    public async Task SubscribeAsync(
        DirectExchange exchange,
        DirectExchange quarantine,
        Func<string, CancellationToken, Task> processor,
        CancellationToken ct
    )
    {
        var channel = await EnsureChannelAsync();
        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, ea) =>
            await TryReceiveAsync(exchange, quarantine, processor, ct, ea, channel);

        await channel.BasicConsumeAsync(
            exchange.Queue,
            autoAck: false,
            consumer,
            cancellationToken: ct
        );
    }

    private async Task TryReceiveAsync(
        DirectExchange exchange,
        DirectExchange quarantine,
        Func<string, CancellationToken, Task> processor,
        CancellationToken ct,
        BasicDeliverEventArgs ea,
        IChannel channel
    )
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);

        try
        {
            await processor(message, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error processing message {Message} from {Queue}",
                message,
                exchange.Queue
            );

            await PublishAsync(message, quarantine);
        }
        finally
        {
            await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: ct);
        }
    }

    public async Task PublishAsync(string message, DirectExchange exchange)
    {
        if (_connection == null || _channel == null)
            await EnsureChannelAsync();

        if (_connection == null || _channel == null)
            return;

        var body = Encoding.UTF8.GetBytes(message);
        var props = new BasicProperties();

        await _channel.BasicPublishAsync(exchange.Exchange, exchange.Route, true, props, body);
    }

    private async Task<IChannel> EnsureChannelAsync()
    {
        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        if (_connection == null || _channel == null)
            throw new InvalidOperationException("Could not create RabbitMQ channel");

        return _channel;
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null)
            await _channel.CloseAsync();

        if (_connection != null)
            await _connection.CloseAsync();
    }
}
