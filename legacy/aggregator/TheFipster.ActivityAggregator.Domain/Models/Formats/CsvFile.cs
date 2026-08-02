namespace TheFipster.ActivityAggregator.Domain.Models.Formats
{
    public class CsvFile
    {
        private readonly string _filepath;
        private readonly string _separator;

        public CsvFile(string filepath, string separator)
        {
            if (!File.Exists(filepath))
                throw new ArgumentException($"File {filepath} doesn't exist.");

            _filepath = filepath;
            _separator = separator;
        }

        public IEnumerable<string[]> ReadLines() =>
            File.ReadAllLines(_filepath).Select(a => a.Split(_separator));
    }
}
