using System.Text.Json;
using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Contracts;
using Fip.Strive.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Domain.Exceptions;
using Fip.Strive.Ingestion.Domain.Models;

namespace Fip.Strive.Ingestion.Application.Vendors.Polar.Flow.Calendar;

public class PolarFlowCalendarItemsExtractor : IFileExtractor
{
    public DataSources Source => DataSources.PolarFlowCalendarItems;
    public int ExtractorVersion => 1;

    public List<FileExtraction> Extract(string filepath, DateTime? _ = null)
    {
        var calendarItems = DeserializeCalendar(filepath);
        var results = ExtractData(filepath, calendarItems).ToList();

        if (results.Any())
            return results;

        throw new ExtractionException(filepath, "Couldn't find any data.");
    }

    private static PolarFlowCalendarItems DeserializeCalendar(string filepath)
    {
        var json = File.ReadAllText(filepath);
        var calendarItems =
            JsonSerializer.Deserialize<PolarFlowCalendarItems>(json)
            ?? throw new ArgumentException("Couldn't parse polar takeout sleep score.");
        return calendarItems;
    }

    private List<FileExtraction> ExtractData(string filepath, PolarFlowCalendarItems calendarItems)
    {
        var results = new List<FileExtraction>();

        AppendBodyweights(results, filepath, calendarItems);

        return results;
    }

    private void AppendBodyweights(
        List<FileExtraction> results,
        string filepath,
        PolarFlowCalendarItems calendarItems
    )
    {
        if (calendarItems.Weights == null)
            return;

        foreach (var item in calendarItems.Weights)
        {
            var extraction = new FileExtraction(
                Source,
                filepath,
                item.Timestamp.Date,
                DataKind.Day
            );

            extraction.AddAttribute(Parameters.Bodyweight, item.Value);
            results.Add(extraction);
        }
    }
}
