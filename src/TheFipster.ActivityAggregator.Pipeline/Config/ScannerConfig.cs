namespace TheFipster.ActivityAggregator.Pipeline.Config;

public class ScannerConfig
{
    public const string ConfigSectionName = "Stages:Scanner";

    public List<string> Include { get; private set; } = new();
}
