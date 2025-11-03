using Fip.Strive.Core.Application.Features.Config.Models;

namespace Fip.Strive.Harvester.Application.Features.Assimilate;

public class AssimilateConfig() : BaseConfig("Features:Assimilate")
{
    public string Path { get; set; } = "data/files/assimilate";
}
