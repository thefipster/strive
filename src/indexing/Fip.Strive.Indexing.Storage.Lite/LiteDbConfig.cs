using Fip.Strive.Core.Application.Features.Config.Models;

namespace Fip.Strive.Indexing.Storage.Lite;

public class LiteDbConfig() : BaseConfig("Indexing:LiteDb")
{
    public string DatabasePath { get; set; } = "data/db/index.db";
}
