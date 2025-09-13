using System.Text.Json;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Polar.Domain;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Polar
{
    public class PolarTakeoutPpiImporter : IFileImporter
    {
        public string Type => "polar_takeout_ppi";
        public DataSources Source => DataSources.PolarTakeoutPpi;

        public ImportClassification? Classify(string filepath)
        {
            var peeker = new FilePeeker(filepath);

            var result = peeker.ReadChars(256);
            if (!result.Contains("\"devicePpiSamplesList\""))
                return null;

            var date = peeker.ReadTokens("date");
            if (string.IsNullOrWhiteSpace(date))
                return null;

            return new ImportClassification
            {
                Filepath = filepath,
                Source = Source,
                Filetype = Type,
                Datetime = DateTime.Parse(date),
                Datetype = DateRanges.Month,
            };
        }

        public List<FileExtraction> Extract(ArchiveIndex file)
        {
            var json = File.ReadAllText(file.Filepath);
            var ppiSamples =
                JsonSerializer.Deserialize<List<PolarTakeoutPpiSamples>>(json)
                ?? throw new ArgumentException("Couldn't parse polar takeout ppi samples.");

            var result = new List<FileExtraction>();

            foreach (var day in ppiSamples)
            {
                var date = day.Date;
                var extraction = new FileExtraction(
                    DataSources.PolarTakeoutPpi,
                    file.Filepath,
                    date.Date,
                    DateRanges.Day
                );
                var maxPulse = int.MinValue;
                var minPulse = int.MaxValue;

                foreach (var list in day.DevicePpiSamplesList)
                {
                    var series = FileExtraction.EmptySeries;
                    var timestampSeries = new List<string>();
                    var rrSeries = new List<string>();

                    foreach (var sample in list.PpiSamples)
                    {
                        if (maxPulse < sample.PulseLength)
                            maxPulse = sample.PulseLength;

                        if (minPulse > sample.PulseLength)
                            minPulse = sample.PulseLength;

                        timestampSeries.Add(sample.SampleDateTime.ToString("s"));
                        rrSeries.Add(sample.PulseLength.ToString());
                    }

                    extraction.Series.Add(Parameters.Timestamp, timestampSeries);
                    extraction.Series.Add(Parameters.Rr, rrSeries);

                    extraction.Attributes.Add(Parameters.MaxRr, maxPulse.ToString());
                    extraction.Attributes.Add(Parameters.MinRr, minPulse.ToString());
                }

                result.Add(extraction);
            }

            return result;
        }
    }
}
