namespace TheFipster.ActivityAggregator.Api.Setup.Configs;

public class ImportConfig
{
    public const string ConfigSectionName = "Api:Import";

    public string ImportFileExtensionFilter { get; set; } = ".zip";

    public string UploadDirectoryPath { get; set; } = "upload";

    public string UnzipDirectoryPath { get; set; } = "unzip";

    public string ConvergeDirectoryPath { get; set; } = "converge";

    public string MergeDirectoryPath { get; set; } = "merge";
}
