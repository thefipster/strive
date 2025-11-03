using Fip.Strive.Core.Application.Features.Config.Models;

namespace Fip.Strive.Harvester.Application.Core.Indexing;

public class IndexConfig() : BaseConfig("Features:Index")
{
    public string DatabasePath { get; set; } = "data/db/index.db";
}
