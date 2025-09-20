namespace TheFipster.ActivityAggregator.Pipeline.Config;

public class UnifierConfig
{
    public const string ConfigSectionName = "Stages:Unifier";

    public string OutputDirectory { get; set; } = "unified";

    public int MaxDegreeOfParallelism { get; set; } = 2;
}
