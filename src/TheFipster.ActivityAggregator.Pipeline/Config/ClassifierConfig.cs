namespace TheFipster.ActivityAggregator.Pipeline.Config;

public class ClassifierConfig
{
    public const string ConfigSectionName = "Stages:Classifier";

    public int MaxDegreeOfParallelism { get; set; } = 2;
}
