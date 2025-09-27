namespace TheFipster.ActivityAggregator.Domain.Configs;

public class ApiConfig
{
    public const string ConfigSectionName = "Api";

    public string ImportFileExtensionFilter { get; set; }
    public string UploadDirectoryPath { get; set; }
}
