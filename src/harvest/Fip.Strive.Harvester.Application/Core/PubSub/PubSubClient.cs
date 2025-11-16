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
        Func<string, CancellationToken, Task> processor,
        CancellationToken ct
    )
    {
        if (_connection == null || _channel == null)
            await ConnectAsync();

        if (_connection == null || _channel == null)
            return;

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                await processor(message, ct);
                await _channel.BasicAckAsync(
                    ea.DeliveryTag,
                    multiple: false,
                    cancellationToken: ct
                );
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error processing message {Message} from {Queue}",
                    message,
                    exchange.Queue
                );
                await _channel.BasicNackAsync(ea.DeliveryTag, false, true, ct);
            }
        };

        await _channel.BasicConsumeAsync(
            exchange.Queue,
            autoAck: false,
            consumer,
            cancellationToken: ct
        );
    }

    public async Task PublishAsync(string message, DirectExchange exchange)
    {
        if (_connection == null || _channel == null)
            await ConnectAsync();

        if (_connection == null || _channel == null)
            return;

        var body = Encoding.UTF8.GetBytes(message);
        var props = new BasicProperties();

        await _channel.BasicPublishAsync(exchange.Exchange, exchange.Route, true, props, body);
    }

    private async Task ConnectAsync()
    {
        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null)
            await _channel.CloseAsync();

        if (_connection != null)
            await _connection.CloseAsync();
    }
}
