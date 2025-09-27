namespace TheFipster.ActivityAggregator.Pipeline.Config;

public class NewPipelineConfig
{
    public const string ConfigSectionName = "NewPipeline";

    public string ImportDirectory { get; set; }
    public string UnzipDirectory { get; set; }
}
