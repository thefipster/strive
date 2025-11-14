using Fip.Strive.Core.Application.Features.Config.Models;

namespace Fip.Strive.Harvester.Domain;

public class HarvestConfig
{
    public const string ConfigSectionName = "Harvest";

    public string UploadPath { get; set; } = "data/files/upload";
    public string ImportPath { get; set; } = "data/files/import";
    public bool ImportOverwrite { get; set; } = false;
}
