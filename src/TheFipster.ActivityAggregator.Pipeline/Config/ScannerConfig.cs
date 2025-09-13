namespace TheFipster.ActivityAggregator.Pipeline.Config;

public class ScannerConfig
{
    public const string ConfigSectionName = "ScannerStage";

    public List<string> Include { get; private set; } = new();
}
