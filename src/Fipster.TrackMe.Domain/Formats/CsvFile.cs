namespace Fipster.TrackMe.Domain.Formats
{
    public class CsvFile
    {
        private readonly string filepath;
        private readonly string separator;

        public CsvFile(string filepath, string separator)
        {
            if (!File.Exists(filepath))
                throw new ArgumentException($"File {filepath} doesn't exist.");

            this.filepath = filepath;
            this.separator = separator;
        }

        public IEnumerable<string[]> ReadLines() =>
            File.ReadAllLines(filepath).Select(a => a.Split(separator));
    }
}
