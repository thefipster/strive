using System.Text.Json;
using Fip.Strive.Harvester.Application.Core.Signals;

namespace Fip.Strive.Harvester.Application.Core.PubSub.Models;

public class QuarantinedMessage
{
    public required string Payload { get; set; }
    public int SignalType { get; set; }
    public required string ErrorType { get; set; }
    public required string ErrorMessage { get; set; }
    public required string Stacktrace { get; set; }

    public string ToJson() => JsonSerializer.Serialize(this);

    public static QuarantinedMessage From(string payload, Signal? signal, Exception exception) =>
        new()
        {
            Payload = payload,
            SignalType = signal?.Type ?? 0,
            ErrorType = exception.GetType().FullName ?? "Unknown Error",
            ErrorMessage = exception.Message,
            Stacktrace = exception.StackTrace ?? "No Stacktrace available.",
        };

    public static QuarantinedMessage? FromJson(string message) =>
        JsonSerializer.Deserialize<QuarantinedMessage>(message);
}
