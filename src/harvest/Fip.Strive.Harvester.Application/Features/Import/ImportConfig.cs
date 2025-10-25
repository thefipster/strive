using Fip.Strive.Harvester.Application.Core.Config.Models;

namespace Fip.Strive.Harvester.Application.Features.Import;

public class ImportConfig() : BaseConfig("Features:Import")
{
    public string Path { get; set; } = "data/files/import";
}
