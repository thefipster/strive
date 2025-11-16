using System.ComponentModel.DataAnnotations;
using Fip.Strive.Core.Application.Features.Config.Models;

namespace Fip.Strive.Harvester.Domain;

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
