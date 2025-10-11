namespace TheFipster.ActivityAggregator.Domain.Models.Importing;

public class DirectoryStats
{
    public required string OutputPath { get; set; }
    public int FileCount { get; set; }
    public long Size { get; set; }
}
