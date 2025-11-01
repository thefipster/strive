using System.Text.Json;
using Fip.Strive.Core.Domain.Exceptions;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Components;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
using Fip.Strive.Core.Ingestion.Contracts;

namespace Fip.Strive.Core.Ingestion.Vendors.Polar.Flow.SkinContact
{
    public class PolarFlowGenericPeriodSkinContactExtractor : IFileExtractor
    {
        public DataSources Source => DataSources.PolarFlowGenericPeriodSkinContact;

        public int ExtractorVersion => 1;

        public List<FileExtraction> Extract(string filepath, DateTime? date = null)
        {
            if (date == null)
                throw new ExtractionException(
                    filepath,
                    "Date is required for polar flow generic period skin contact files."
                );

            var json = File.ReadAllText(filepath);
            var tempData =
                JsonSerializer.Deserialize<PolarFlowGenericPeriodSkinContact>(json)
                ?? throw new ExtractionException(
                    filepath,
                    "Couldn't parse polar takeout generic period skin contact file."
                );

            if (
                tempData.Data?.SkinContactChanges == null
                || tempData.Data.SkinContactChanges.Count == 0
            )
                throw new ExtractionException(filepath, "Couldn't find any skin contact changes.");

            var result = new FileExtraction(Source, filepath, date.Value.Date, DataKind.Day);
            result.Series.Add(Parameters.Timestamp, []);
            result.Series.Add(Parameters.DeviceWorn, []);

            foreach (var sample in tempData.Data.SkinContactChanges)
            {
                var temperature = sample.SkinContact.GetValueOrDefault().ToString();
                var deltaMs = long.Parse(sample.RecordingTimeDeltaMilliseconds ?? "-1");
                var timestamp = date
                    .Value.Date.AddMilliseconds(deltaMs)
                    .ToString(DateHelper.MillisecondFormat);

                result.Series[Parameters.Timestamp].Add(timestamp);
                result.Series[Parameters.DeviceWorn].Add(temperature);
            }

            return [result];
        }
    }
}
