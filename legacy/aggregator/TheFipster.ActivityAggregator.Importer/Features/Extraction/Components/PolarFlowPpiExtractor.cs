using System.Text.Json;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Domain.Vendor.Polar.Flow;
using TheFipster.ActivityAggregator.Importer.Features.Extraction.Components.Contracts;

namespace TheFipster.ActivityAggregator.Importer.Features.Extraction.Components
{
    public class PolarFlowPpiExtractor : IFileExtractor
    {
        public DataSources Source => DataSources.PolarFlowPpi;
        public int ExtractorVersion => 1;

        public List<FileExtraction> Extract(ExtractionRequest file)
        {
            var json = File.ReadAllText(file.Filepath);
            var ppiSamples =
                JsonSerializer.Deserialize<List<PolarFlowPpiSamples>>(json)
                ?? throw new ArgumentException("Couldn't parse polar takeout ppi samples.");

            var result = new List<FileExtraction>();

            foreach (var day in ppiSamples)
            {
                var date = day.Date;
                var extraction = new FileExtraction(
                    DataSources.PolarFlowPpi,
                    file.Filepath,
                    date.Date,
                    DateRanges.Day
                );
                var maxPulse = int.MinValue;
                var minPulse = int.MaxValue;

                foreach (var list in day.DevicePpiSamplesList)
                {
                    var timestampSeries = new List<string>();
                    var rrSeries = new List<string>();

                    foreach (var sample in list.PpiSamples)
                    {
                        if (maxPulse < sample.PulseLength)
                            maxPulse = sample.PulseLength;

                        if (minPulse > sample.PulseLength)
                            minPulse = sample.PulseLength;

                        timestampSeries.Add(
                            sample.SampleDateTime.ToString(DateHelper.SecondFormat)
                        );
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
