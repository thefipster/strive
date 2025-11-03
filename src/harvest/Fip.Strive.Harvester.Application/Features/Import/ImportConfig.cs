using Fip.Strive.Core.Application.Features.Config.Models;

namespace Fip.Strive.Harvester.Application.Features.Import;

public class ImportConfig() : BaseConfig("Features:Import")
{
    public string Path { get; set; } = "data/files/import";
}
