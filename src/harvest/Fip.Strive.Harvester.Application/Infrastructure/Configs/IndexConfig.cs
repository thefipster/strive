using Fip.Strive.Harvester.Application.Core.Config.Models;

namespace Fip.Strive.Harvester.Application.Infrastructure.Configs;

public class IndexConfig() : BaseConfig("Features:Index")
{
    public string DatabasePath { get; set; } = "data/db/index.db";
}
