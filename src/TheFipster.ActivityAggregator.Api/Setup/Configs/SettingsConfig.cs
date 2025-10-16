namespace TheFipster.ActivityAggregator.Api.Setup.Configs;

public class SettingsConfig
{
    public const string ConfigSectionName = "Api:Settings";

    public int MaxDegreeOfParallelism { get; set; } = Environment.ProcessorCount / 2;
}
