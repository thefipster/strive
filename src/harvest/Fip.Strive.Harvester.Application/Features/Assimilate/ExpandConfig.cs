using Fip.Strive.Harvester.Application.Core.Config.Models;

namespace Fip.Strive.Harvester.Application.Features.Assimilate;

public class AssimilateConfig() : BaseConfig("Features:Assimilate")
{
    public string Path { get; set; } = "data/files/assimilate";
}
