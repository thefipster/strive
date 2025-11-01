using System.Globalization;
using System.Text.Json;
using Fip.Strive.Core.Domain.Exceptions;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Components;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
using Fip.Strive.Core.Ingestion.Contracts;

namespace Fip.Strive.Core.Ingestion.Vendors.TheFipsterApp.Bodyweight;

public class TheFipsterAppWeightExtractor : IFileExtractor
{
    public int ExtractorVersion => 1;
    public DataSources Source => DataSources.TheFipsterAppWeight;

    public List<FileExtraction> Extract(string filepath, DateTime? _ = null)
    {
        var json = File.ReadAllText(filepath);
        var weights = JsonSerializer.Deserialize<List<TheFipsterAppWeightEntry>>(json);

        if (weights == null)
            throw new ExtractionException(filepath, "Could not read file.");

        if (weights.Count == 0)
            throw new ExtractionException(filepath, "No weights found.");

        var dailyWeights = weights.GroupBy(x => x.Date.Date);
        var results = new List<FileExtraction>();

        foreach (var group in dailyWeights)
        {
            var date = group.Key;
            var result = new FileExtraction(
                DataSources.TheFipsterAppWeight,
                filepath,
                date,
                DataKind.Day
            );
            result.AddSeries(Parameters.Bodyweight);
            result.AddSeries(Parameters.Timestamp);

            foreach (var item in group)
            {
                var timestamp = item.Date;
                var weight = item.Weight.ToString(CultureInfo.InvariantCulture);

                result.Series[Parameters.Bodyweight].Add(weight);
                result
                    .Series[Parameters.Timestamp]
                    .Add(timestamp.ToString(DateHelper.SecondFormat));

                var weightEvent = new UnifiedEvent(
                    EventTypes.Bodyweight,
                    timestamp,
                    item.Reason,
                    Parameters.Bodyweight,
                    weight
                );

                result.Events.Add(weightEvent);
            }

            result.AddAttribute(
                Parameters.MinBodyweight,
                group.Min(x => x.Weight).ToString(CultureInfo.InvariantCulture)
            );

            result.AddAttribute(
                Parameters.MaxBodyweight,
                group.Max(x => x.Weight).ToString(CultureInfo.InvariantCulture)
            );

            results.Add(result);
        }

        return results;
    }
}
