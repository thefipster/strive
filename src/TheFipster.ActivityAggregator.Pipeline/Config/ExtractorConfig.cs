namespace TheFipster.ActivityAggregator.Pipeline.Config;

public class ExtractorConfig
{
    public const string ConfigSectionName = "Stages:Extractor";

    public string OutputDirectory { get; set; } = "extraction";

    public int MaxDegreeOfParallelism { get; set; } = 2;
}
