using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;

namespace Fip.Strive.Core.Domain.Schemas.Ingestion.Models
{
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
}
