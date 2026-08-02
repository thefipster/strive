using System.Text.Json;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Domain.Vendor.Polar.Flow;
using TheFipster.ActivityAggregator.Importer.Features.Extraction.Components.Contracts;

namespace TheFipster.ActivityAggregator.Importer.Features.Extraction.Components
{
    public class PolarFlowGenericPeriodSkinContactExtractor : IFileExtractor
    {
        public DataSources Source => DataSources.PolarFlowGenericPeriodSkinContact;
        public int ExtractorVersion => 1;

        public List<FileExtraction> Extract(ExtractionRequest file)
        {
            var json = File.ReadAllText(file.Filepath);
            var tempData =
                JsonSerializer.Deserialize<PolarFlowGenericPeriodSkinContact>(json)
                ?? throw new ExtractionException(
                    file.Filepath,
                    "Couldn't parse polar takeout generic period skin contact file."
                );

            if (
                tempData.Data?.SkinContactChanges == null
                || tempData.Data.SkinContactChanges.Count == 0
            )
                throw new ExtractionException(
                    file.Filepath,
                    "Couldn't find any skin contact changes."
                );

            var result = new FileExtraction(Source, file.Filepath, file.Date, DateRanges.Day);
            result.Series.Add(Parameters.Timestamp, []);
            result.Series.Add(Parameters.DeviceWorn, []);

            foreach (var sample in tempData.Data.SkinContactChanges)
            {
                var temperature = sample.SkinContact.GetValueOrDefault().ToString();
                var deltaMs = long.Parse(sample.RecordingTimeDeltaMilliseconds ?? "-1");
                var timestamp = file
                    .Date.AddMilliseconds(deltaMs)
                    .ToString(DateHelper.MillisecondFormat);

                result.Series[Parameters.Timestamp].Add(timestamp);
                result.Series[Parameters.DeviceWorn].Add(temperature);
            }

            return [result];
        }
    }
}
