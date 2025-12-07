using System.Text.Json;

namespace Fip.Strive.Harvester.Application.Core.PubSub.Models;

public class QuarantinedMessage
{
    public required string Payload { get; set; }
    public required string ErrorType { get; set; }
    public required string ErrorMessage { get; set; }
    public required string Stacktrace { get; set; }

    public string ToJson() => JsonSerializer.Serialize(this);

    public static QuarantinedMessage From(string payload, Exception exception) =>
        new()
        {
            Payload = payload,
            ErrorType = exception.GetType().FullName ?? "Unknown Error",
            ErrorMessage = exception.Message,
            Stacktrace = exception.StackTrace ?? "No Stacktrace available.",
        };
}
