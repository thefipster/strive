namespace Fip.Strive.Harvester.Application;

public class IndexConfig
{
    public const string ConfigSectionName = "Features:Index";

    public string DatabasePath { get; set; } = "data/db/index.db";
}
