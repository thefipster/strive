using System.ComponentModel.DataAnnotations;

namespace Fip.Strive.Harvester.Pipeline.Unzipper.Cli;

public class Config
{
    public const string ConfigSectionName = "Worker";

    [Required]
    public required string Path { get; set; }

    public bool Overwrite { get; set; } = false;
}
