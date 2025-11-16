using System.ComponentModel.DataAnnotations;

namespace Fip.Strive.Harvester.Pipeline.Classify.Cli;

public class Config
{
    public const string ConfigSectionName = "Worker";

    [Required]
    public required string Path { get; set; }
}
