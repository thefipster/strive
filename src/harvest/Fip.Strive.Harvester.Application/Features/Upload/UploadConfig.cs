using Fip.Strive.Harvester.Application.Core.Config.Models;

namespace Fip.Strive.Harvester.Application.Features.Upload;

public class UploadConfig() : BaseConfig("Features:Upload")
{
    public string Path { get; set; } = "data/files/upload";
}
