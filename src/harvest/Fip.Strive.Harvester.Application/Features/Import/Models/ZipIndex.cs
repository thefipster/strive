using System.Security.Cryptography;

namespace Fip.Strive.Harvester.Application.Features.Import.Models;

public class ZipIndex
{
    public required string Hash { get; set; }
    public Dictionary<string, DateTime> Files { get; set; } = new();
    public DateTime UploadedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid SignalId { get; set; }

    public void AddFile(string filename) => Files.Add(filename, DateTime.UtcNow);

    public static ZipIndex From(Guid signalId, string hash, DateTime uploadedAt) =>
        new()
        {
            SignalId = signalId,
            Hash = hash,
            UploadedAt = uploadedAt,
        };
}
