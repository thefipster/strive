using Fip.Strive.Core.Application.Features.Config.Models;

namespace Fip.Strive.Harvester.Application.Features.Expand;

public class ExpandConfig() : BaseConfig("Features:Expand")
{
    public string Path { get; set; } = "data/files/expand";
}
