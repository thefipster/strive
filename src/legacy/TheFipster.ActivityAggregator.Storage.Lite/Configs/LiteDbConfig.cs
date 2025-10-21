namespace TheFipster.ActivityAggregator.Storage.Lite.Configs;

public class LiteDbConfig
{
    public const string ConfigSectionName = "LiteDb";

    public string IndexFile { get; set; } = "index.litedb";
}
