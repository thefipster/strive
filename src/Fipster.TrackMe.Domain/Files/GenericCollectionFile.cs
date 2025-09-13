namespace Fipster.TrackMe.Domain
{
    public class GenericCollectionFile<T>
    {
        public DateTime Date { get; set; }
        public DateRanges Range { get; set; }
        public FileTypes Type { get; set; }
        public IEnumerable<T> Data { get; set; }
        public string Source { get; set; }
    }
}
