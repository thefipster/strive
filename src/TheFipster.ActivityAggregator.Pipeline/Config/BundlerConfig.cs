namespace TheFipster.ActivityAggregator.Pipeline.Config;

public class BundlerConfig
{
    public const string ConfigSectionName = "Stages:Bundler";

    public string ImportDirectory { get; set; } = "extraction";

    public int MaxDegreeOfParallelism { get; set; } = 2;
}
