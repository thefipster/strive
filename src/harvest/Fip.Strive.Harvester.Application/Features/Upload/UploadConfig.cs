using System.ComponentModel.DataAnnotations;

namespace Fip.Strive.Harvester.Application.Features.Upload;

public class UploadConfig
{
    public const string ConfigSectionName = "Features:Upload";

    [Required]
    public required string Path { get; set; }

    public int ReportIntervalInMs { get; set; } = 250;

    public int BufferSizeInKb { get; set; } = 80;
}
