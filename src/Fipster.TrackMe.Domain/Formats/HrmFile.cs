using System.Text;
using Fipster.TrackMe.Domain.Tools;

namespace Fipster.TrackMe.Domain.Formats
{
    public class HrmFile
    {
        private readonly string filepath;

        public HrmFile(string filepath)
        {
            if (!File.Exists(filepath))
                throw new ArgumentException($"File {filepath} doesn't exist.");

            var header = new FilePeeker(filepath).ReadLines(1);
            if (header == null || header.Count() == 0 || header.First() != "[Params]")
                throw new ArgumentException($"File {filepath} is not a hrm file.");

            this.filepath = filepath;
        }

        public Dictionary<string, string> GetParams()
        {
            var props = new Dictionary<string, string>();

            const int BufferSize = 128;
            using (var fileStream = File.OpenRead(filepath))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
            {
                string line;
                string section = string.Empty;
                while ((line = streamReader.ReadLine()) != null)
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
            }

            return props;
        }

        public List<List<int>> GetSamples()
        {
            var series = new List<List<int>>();

            const int BufferSize = 128;
            using (var fileStream = File.OpenRead(filepath))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
            {
                string line;
                string section = string.Empty;
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (line == null || string.IsNullOrWhiteSpace(line))
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
            }

            return series;
        }
    }
}
