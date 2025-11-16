using System.Text.Json;
using Fip.Strive.Harvester.Domain.Signals;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Harvester.Domain.Indexes;

public class DataIndex
{
    public required string Hash { get; set; }
    public required string Filepath { get; set; }
    public required string ParentHash { get; set; }
    public required string ParentFilepath { get; set; }
    public required DataSources Source { get; set; }
    public required bool IsDay { get; set; }
    public required DateTime Timestamp { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string ToJson() => JsonSerializer.Serialize(this);

    public static DataIndex From(
        string filepath,
        string hash,
        bool isDay,
        DateTime timestamp,
        TypedSignal inSignal
    ) =>
        new DataIndex
        {
            Filepath = filepath,
            Hash = hash,
            IsDay = isDay,
            Timestamp = timestamp,
            ParentFilepath = inSignal.Filepath,
            ParentHash = inSignal.Hash,
            Source = inSignal.Source,
        };
}
