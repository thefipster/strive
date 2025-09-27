namespace TheFipster.ActivityAggregator.Domain.Configs;

public class ScannerConfig
{
    public const string ConfigSectionName = "Service:Scanner";

    public ScannerConfig() { }

    public ScannerConfig(params string[] excludedFileExtensions) =>
        ExcludedFileExtensions = excludedFileExtensions.ToList();

    public List<string> ExcludedFileExtensions { get; set; } = [];
}
