namespace Fip.Strive.Application.Features.Import.Models;

public readonly record struct ImportProgress(int FilesProcessed, int TotalFiles, string CurrentPath)
{
    public double Percent => TotalFiles == 0 ? 0 : 100d * FilesProcessed / TotalFiles;
}
