using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Extraction;

namespace TheFipster.ActivityAggregator.Domain.Models.Scanner
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

        public string[] DatePieces =>
            Datetype switch
            {
                DateRanges.Time =>
                [
                    Datetime.ToString("yyyy"),
                    Datetime.ToString("MM"),
                    Datetime.ToString("dd"),
                    Datetime.ToString("HH_mm"),
                ],
                DateRanges.Day =>
                [
                    Datetime.ToString("yyyy"),
                    Datetime.ToString("MM"),
                    Datetime.ToString("dd"),
                ],
                DateRanges.Month => [Datetime.ToString("yyyy"), Datetime.ToString("MM")],
                DateRanges.Year => [Datetime.ToString("yyyy")],
                DateRanges.AllTime => ["all"],
                _ => throw new InvalidOperationException("Not supported"),
            };

        public Classification ToClassification() => new Classification(Source, Datetime, Datetype);
    }
}
