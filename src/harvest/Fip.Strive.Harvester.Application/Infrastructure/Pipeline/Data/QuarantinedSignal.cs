using Fip.Strive.Harvester.Application.Core.PubSub.Models;

namespace Fip.Strive.Harvester.Application.Infrastructure.Pipeline.Data;

public class QuarantinedSignal
{
    private QuarantinedSignal() { }

    public Guid Id { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public string? Stacktrace { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorType { get; set; }
    public int SignalType { get; set; }
    public string? Payload { get; set; }

    public static QuarantinedSignal From(QuarantinedMessage message) =>
        new()
        {
            Payload = message.Payload,
            SignalType = message.SignalType,
            ErrorType = message.ErrorType,
            ErrorMessage = message.ErrorMessage,
            Stacktrace = message.Stacktrace,
        };
}
