using System.Text.Json;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Importing;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Domain.Vendor.Polar.Flow;
using TheFipster.ActivityAggregator.Importer.Abstractions;

namespace TheFipster.ActivityAggregator.Importer.Polar
{
    public class PolarTakeoutPpiImporter : IFileClassifier, IFileExtractor
    {
        public DataSources Source => DataSources.PolarFlowPpi;
        public int ClassifierVersion => 1;
        public int ExtractorVersion => 1;

        private readonly HashSet<string> _required = ["date", "devicePpiSamplesList"];

        public ImportClassification Classify(FileProbe probe)
        {
            var values = probe.JsonValues;

            if (values == null)
                throw new ClassificationException(
                    probe.Filepath,
                    Source,
                    "Couldn't find valid json."
                );

            if (!_required.IsSubsetOf(values.Keys))
                throw new ClassificationException(
                    probe.Filepath,
                    Source,
                    "Couldn't find required properties."
                );

            var date = values["date"];
            if (string.IsNullOrWhiteSpace(date))
                throw new ClassificationException(
                    probe.Filepath,
                    Source,
                    "Couldn't find date value."
                );

            return new ImportClassification
            {
                Filepath = probe.Filepath,
                Source = Source,
                Datetime = DateTime.Parse(date),
                Datetype = DateRanges.Month,
            };
        }

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
