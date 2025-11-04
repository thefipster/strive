using Fip.Strive.Core.Application.Features.Config.Models;

namespace Fip.Strive.Indexing.Application;

public class IndexConfig() : BaseConfig("Features:Index")
{
    public string DatabasePath { get; set; } = "data/db/index.db";
}
