namespace TheFipster.ActivityAggregator.Pipeline.Config;

public class ClassifierConfig
{
    public const string ConfigSectionName = "Stages:Classifier";

    public int MaxDegreeOfParallelismDequeing { get; set; } = 2;
    public int MaxDegreeOfParallelismClassifying { get; set; } = 2;
}
