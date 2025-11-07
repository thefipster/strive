using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Domain.Models;

public class ImportClassification
{
    public ImportClassification()
    {
        Datetime = DateTime.MinValue;
    }

    public required string Filepath { get; set; }
    public DataSources Source { get; set; }
    public DateTime Datetime { get; set; }
    public DateRanges Datetype { get; set; }

    public Classification ToClassification() => new Classification(Source, Datetime, Datetype);
}
