using System.Globalization;
using System.Text.Json;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Models.Files;
using TheFipster.ActivityAggregator.Domain.Models.Requests;
using TheFipster.ActivityAggregator.Domain.Vendor.Polar.Flow;
using TheFipster.ActivityAggregator.Importer.Features.Extraction.Components.Contracts;

namespace TheFipster.ActivityAggregator.Importer.Features.Extraction.Components;

public class PolarFlowCalendarItemsExtractor : IFileExtractor
{
    public DataSources Source => DataSources.PolarFlowCalendarItems;
    public int ExtractorVersion => 1;

    public List<FileExtraction> Extract(ExtractionRequest file)
    {
        var json = File.ReadAllText(file.Filepath);
        var calendarItems =
            JsonSerializer.Deserialize<PolarFlowCalendarItems>(json)
            ?? throw new ArgumentException("Couldn't parse polar takeout sleep score.");

        var result = new List<FileExtraction>();

        if (calendarItems.Weights == null)
            return result;

        foreach (
            var item in calendarItems
                .Weights.GroupBy(w => w.Timestamp.Date)
                .Select(g => g.First())
                .ToList()
        )
        {
            var attributes = FileExtraction.EmptyAttributes;
            attributes.Add(
                Parameters.Bodyweight,
                item.Value.ToString(CultureInfo.InvariantCulture)
            );

            var extraction = new FileExtraction(
                Source,
                file.Filepath,
                item.Timestamp.Date,
                DateRanges.Day,
                attributes
            );
            result.Add(extraction);
        }

        return result;
    }
}
