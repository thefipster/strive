namespace Fip.Strive.Ingestion.Domain.Models;

public class ExtractionResponse
{
    public List<FileExtraction> Extractions { get; set; } = [];
    public int Version { get; set; }
}
