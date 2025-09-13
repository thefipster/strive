namespace Fipster.TrackMe.Importer.Cli.Models
{
    public class ScanResult
    {
        public int Total { get; set; }
        public int Matches { get; set; }
        public int Misses { get; set; }
        public int Doubles { get; set; }
        public int Percentage => (int)Math.Floor(100.0 * Matches / Total);
        public bool Valid => Percentage == 100 && Misses == 0 && Doubles == 0;
    }
}
