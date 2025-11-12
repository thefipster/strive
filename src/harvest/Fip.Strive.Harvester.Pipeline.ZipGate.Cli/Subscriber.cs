using System.Text;
using Fip.Strive.Harvester.Domain.Defaults;
using Fip.Strive.Harvester.Pipeline.ZipGate.Cli.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Fip.Strive.Harvester.Pipeline.ZipGate.Cli;

public class Subscriber(
    IConnectionFactory factory,
    IMessageProcessor processor,
    ILogger<Subscriber> logger
) : BackgroundService
{
    private IConnection? _connection;
    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _connection = await factory.CreateConnectionAsync(ct);
        _channel = await _connection.CreateChannelAsync(cancellationToken: ct);

        await _channel.ExchangeDeclareAsync(
            QueueDeclarations.Upload.Exchange,
            ExchangeType.Direct,
            durable: true,
            cancellationToken: ct
        );

        await _channel.QueueDeclareAsync(
            QueueDeclarations.Upload.Queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: ct
        );

        await _channel.QueueBindAsync(
            QueueDeclarations.Upload.Queue,
            QueueDeclarations.Upload.Exchange,
            QueueDeclarations.Upload.Route,
            cancellationToken: ct
        );

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                await processor.ProcessAsync(message, ct);
                await _channel.BasicAckAsync(
                    ea.DeliveryTag,
                    multiple: false,
                    cancellationToken: ct
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing message: {messageText}", message);
                await _channel.BasicNackAsync(ea.DeliveryTag, false, true, ct);
            }
        };

        await _channel.BasicConsumeAsync(
            QueueDeclarations.Upload.Queue,
            autoAck: false,
            consumer,
            cancellationToken: ct
        );
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null)
            await _channel.CloseAsync();

        if (_connection != null)
            await _connection.CloseAsync();
    }
}
