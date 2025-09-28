namespace TheFipster.ActivityAggregator.Domain.Configs;

public class ApiConfig
{
    public const string ConfigSectionName = "Api";

    public string ImportFileExtensionFilter { get; set; } = ".zip";
    public string UploadDirectoryPath { get; set; } = "upload";
    public string UnzipDirectoryPath { get; set; } = "extracts";
    public string ConvergeDirectoryPath { get; set; } = "converge";
}
