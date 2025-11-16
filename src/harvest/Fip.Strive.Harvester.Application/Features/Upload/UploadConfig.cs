using Fip.Strive.Core.Application.Features.Config.Models;

namespace Fip.Strive.Harvester.Application.Features.Upload;

public class UploadConfig() : BaseConfig("Features:Upload")
{
    public string Path { get; set; } = "data/files/upload";

    public int ReportIntervalInMs { get; set; } = 250;

    public int BufferSizeInKb { get; set; } = 80;
}
