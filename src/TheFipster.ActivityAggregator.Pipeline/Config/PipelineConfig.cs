namespace TheFipster.ActivityAggregator.Pipeline.Config;

public class PipelineConfig
{
    public const string ConfigSectionName = "Pipeline";

    public List<string> ImportDirectories { get; set; } = [];
}
