using System.Text;
using TheFipster.ActivityAggregator.Domain.Tools;

namespace TheFipster.ActivityAggregator.Domain.Formats
{
    public class HrmFile
    {
        private readonly string filepath;

        public HrmFile(string filepath)
        {
            if (!File.Exists(filepath))
                throw new ArgumentException($"File {filepath} doesn't exist.");

            var header = new FileProbe(filepath).Lines?.Take(1).ToArray();
            if (header == null || !header.Any() || header.First() != "[Params]")
                throw new ArgumentException($"File {filepath} is not a hrm file.");

            this.filepath = filepath;
        }

        public Dictionary<string, string> GetParams()
        {
            var props = new Dictionary<string, string>();
            const int bufferSize = 128;
            string section = string.Empty;

            using var fileStream = File.OpenRead(filepath);
            using var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, bufferSize);

            while (streamReader.ReadLine() is { } line)
            {
                line = line.Trim();

                if (string.IsNullOrWhiteSpace(line))
                    break;

                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    section = line;
                    continue;
                }

                if (section == "[Params]")
                {
                    var data = line.Split("=");
                    props.Add(data[0], data[1]);
                }
            }

            return props;
        }

        public List<List<int>> GetSamples()
        {
            var series = new List<List<int>>();
            const int bufferSize = 128;
            string section = string.Empty;

            using var fileStream = File.OpenRead(filepath);
            using var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, bufferSize);

            while (streamReader.ReadLine() is { } line)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                line = line.Trim();
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    section = line;
                    continue;
                }

                if (section == "[HRData]")
                {
                    var data = line.Split("\t");

                    if (series.Count == 0)
                        for (int i = 0; i < data.Length; i++)
                            series.Add(new List<int>());

                    for (int i = 0; i < data.Length; i++)
                        series[i].Add(int.Parse(data[i]));
                }
            }

            return series;
        }
    }
}
