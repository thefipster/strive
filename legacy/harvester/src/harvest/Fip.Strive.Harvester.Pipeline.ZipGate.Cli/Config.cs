using System.ComponentModel.DataAnnotations;

namespace Fip.Strive.Harvester.Pipeline.ZipGate.Cli;

public class Config
{
    public const string ConfigSectionName = "Worker";

    [Required]
    public required string Path { get; set; }

    public bool Overwrite { get; set; } = false;
}
