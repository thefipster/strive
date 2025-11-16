using System.Text.Json;
using Fip.Strive.Harvester.Domain.Signals;
using Fip.Strive.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Domain.Models;

namespace Fip.Strive.Harvester.Domain.Indexes;

public class SourceIndex
{
    public required string Hash { get; set; }
    public required string Filepath { get; set; }
    public required string ClassifyHash { get; set; }
    public DataSources Source { get; set; }
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string ToJson() => JsonSerializer.Serialize(this);

    public static SourceIndex From(string classifyHash, FileSignal inSignal) =>
        new SourceIndex
        {
            Hash = inSignal.Hash,
            Filepath = inSignal.Filepath,
            ClassifyHash = classifyHash,
        };

    public static SourceIndex From(
        string classifyHash,
        FileSignal inSignal,
        Classification result
    ) =>
        new SourceIndex
        {
            Hash = inSignal.Hash,
            Filepath = inSignal.Filepath,
            ClassifyHash = classifyHash,
            Source = result.Source,
            Version = result.Version,
        };
}
