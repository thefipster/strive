namespace Fip.Strive.Harvester.Application.Infrastructure.Configs;

public class IndexConfig
{
    public const string ConfigSectionName = "Features:Index";

    public string DatabasePath { get; set; } = "data/db/index.db";
}
