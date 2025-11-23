using System.ComponentModel.DataAnnotations;

namespace Fip.Strive.Harvester.Application;

public class HarvestConfig
{
    public const string ConfigSectionName = "Harvest";

    [Required]
    public required string UploadPath { get; set; }

    [Required]
    public required string ImportPath { get; set; }

    [Required]
    public required string UnzipPath { get; set; }

    public bool ImportOverwrite { get; set; } = false;
}
