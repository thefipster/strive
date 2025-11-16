using System.ComponentModel.DataAnnotations;

namespace Fip.Strive.Harvester.Pipeline.Scanner.Cli;

public class Config
{
    public const string ConfigSectionName = "Worker";

    [Required]
    public required string Path { get; set; }
}
