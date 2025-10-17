using System.Globalization;
using System.Text.Json;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Importing;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Features.Extraction.Components.Contracts;

namespace TheFipster.ActivityAggregator.Importer.Features.Extraction.Components;

public class TheFipsterAppWeightExtractor : IFileExtractor
{
    public int ExtractorVersion => 1;
    public DataSources Source => DataSources.TheFipsterAppWeight;

    public List<FileExtraction> Extract(ExtractionRequest request)
    {
        var json = File.ReadAllText(request.Filepath);
        var weights = JsonSerializer.Deserialize<List<WeightEntry>>(json);

        if (weights == null)
            throw new ExtractionException(request.Filepath, "Could not read file.");

        if (weights.Count == 0)
            throw new ExtractionException(request.Filepath, "No weights found.");

        var dailyWeights = weights.GroupBy(x => x.Date.Date);
        var results = new List<FileExtraction>();

        foreach (var group in dailyWeights)
        {
            var date = group.Key;
            var result = new FileExtraction(
                DataSources.TheFipsterAppWeight,
                request.Filepath,
                date,
                DateRanges.Day
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
