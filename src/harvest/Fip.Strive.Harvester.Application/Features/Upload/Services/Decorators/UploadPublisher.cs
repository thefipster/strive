using System.Text;
using Fip.Strive.Core.Application.Features.FileSystem.Services.Contracts;
using Fip.Strive.Harvester.Application.Features.Upload.Services.Contracts;
using Fip.Strive.Harvester.Domain.Defaults;
using Fip.Strive.Harvester.Domain.Signals;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Fip.Strive.Harvester.Application.Features.Upload.Services.Decorators;

public class UploadPublisher(
    IUploadService component,
    IConnectionFactory factory,
    IFileHasher hasher
) : IUploadService
{
    private readonly DirectExchange _exchange = HarvestPipelineExchange.New(
        SignalTypes.UploadSignal
    );

    public event EventHandler<int>? ProgressChanged
    {
        add => component.ProgressChanged += value;
        remove => component.ProgressChanged -= value;
    }

    public void SetReportInterval(TimeSpan interval) => component.SetReportInterval(interval);

    public async Task<string> SaveUploadAsync(string filename, Stream stream, CancellationToken ct)
    {
        var filepath = await component.SaveUploadAsync(filename, stream, ct);
        var hash = await hasher.HashXx3Async(filepath, ct);

        await using var connection = await factory.CreateConnectionAsync(ct);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: ct);

        await channel.ExchangeDeclareAsync(
            _exchange.Exchange,
            ExchangeType.Direct,
            durable: true,
            cancellationToken: ct
        );

        await channel.QueueDeclareAsync(
            _exchange.Queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: ct
        );

        await channel.QueueBindAsync(
            _exchange.Queue,
            _exchange.Exchange,
            _exchange.Route,
            cancellationToken: ct
        );

        var signal = UploadSignal.From(filepath, hash);
        var message = JsonConvert.SerializeObject(signal);
        var body = Encoding.UTF8.GetBytes(message);
        var props = new BasicProperties();

        await channel.BasicPublishAsync(_exchange.Exchange, _exchange.Route, true, props, body, ct);

        return filepath;
    }
}
