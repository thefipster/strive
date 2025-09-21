namespace TheFipster.ActivityAggregator.Pipeline.Config;

public class ImporterConfig
{
    public const string ConfigSectionName = "Stages:Importer";

    public List<string> ImportDirectories { get; set; } = [];
    public int MaxDegreeOfParallelism { get; set; } = 2;
}
