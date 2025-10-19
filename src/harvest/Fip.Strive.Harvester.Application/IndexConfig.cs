namespace Fip.Strive.Harvester.Application;

public class IndexConfig
{
    public const string ConfigSectionName = "Features:Indexing";

    public string DatabasePath { get; set; } = "data/db/index.db";
}
