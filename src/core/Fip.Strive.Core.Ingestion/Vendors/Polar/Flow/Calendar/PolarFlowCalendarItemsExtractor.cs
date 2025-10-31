using System.Globalization;
using System.Text.Json;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Domain.Schemas.Ingestion.Models;
using Fip.Strive.Core.Ingestion.Contracts;

namespace Fip.Strive.Core.Ingestion.Vendors.Polar.Flow.Calendar;

public class PolarFlowCalendarItemsExtractor : IFileExtractor
{
    public DataSources Source => DataSources.PolarFlowCalendarItems;
    public int ExtractorVersion => 1;

    public List<FileExtraction> Extract(string filepath, DateTime? _ = null)
    {
        var json = File.ReadAllText(filepath);
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
                filepath,
                item.Timestamp.Date,
                DataKind.Day,
                attributes
            );
            result.Add(extraction);
        }

        return result;
    }
}
