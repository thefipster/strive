namespace TheFipster.ActivityAggregator.Pipeline.Config;

public class ScannerConfig
{
    public const string ConfigSectionName = "Stages:Scanner";

    public List<string> ExcludedFileExtensions { get; set; } = [];
    public int MaxDegreeOfParallelism { get; set; } = 2;
}
