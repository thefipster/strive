using System.Text;
using Fip.Strive.Harvester.Application.Core.PubSub.Contracts;
using Fip.Strive.Harvester.Application.Defaults;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Fip.Strive.Harvester.Application.Core.PubSub;

public class SubClient(IConnectionFactory factory, ILogger<SubClient> logger)
    : ISubClient,
        IAsyncDisposable
{
    private IConnection? _connection;
    private IChannel? _channel;

    public event Func<string, CancellationToken, Task>? MessageReceived;

    public async Task SubscribeAsync(DirectExchange exchange, CancellationToken ct)
    {
        var channel = await EnsureChannelAsync(ct);
        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                var handler = MessageReceived;
                if (handler is not null)
                    await handler(message, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error in MessageReceived handler for message from {Queue}",
                    exchange.Queue
                );
            }
            finally
            {
                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: ct);
            }
        };

        await channel.BasicConsumeAsync(
            exchange.Queue,
            autoAck: false,
            consumer,
            cancellationToken: ct
        );
    }

    private async Task<IChannel> EnsureChannelAsync(CancellationToken ct)
    {
        if (_connection is not null && _channel is not null)
            return _channel;

        _connection = await factory.CreateConnectionAsync(ct);
        _channel = await _connection.CreateChannelAsync(cancellationToken: ct);

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
