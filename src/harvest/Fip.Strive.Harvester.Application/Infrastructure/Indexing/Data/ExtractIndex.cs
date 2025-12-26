using System.Text.Json;
using Fip.Strive.Harvester.Application.Core.Signals;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Harvester.Application.Infrastructure.Indexing.Data;

public class ExtractIndex
{
    public required string Hash { get; set; }
    public required string Filepath { get; set; }
    public required DataSources Source { get; set; }

    public int Version { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public static ExtractIndex From(int version, TypedSignal inSignal) =>
        new ExtractIndex
        {
            Version = version,
            Filepath = inSignal.Filepath,
            Hash = inSignal.Hash,
            Source = inSignal.Source,
        };

    public string ToJson() => JsonSerializer.Serialize(this);
}
