using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Contracts;
using Fip.Strive.Ingestion.Application.Models;
using Fip.Strive.Ingestion.Domain.Components;
using Fip.Strive.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Domain.Models;

namespace Fip.Strive.Ingestion.Application.Vendors.TheFipsterApp.Plog;

public class TheFipsterAppPlogExtractor : IFileExtractor
{
    public int ExtractorVersion => 1;
    public DataSources Source => DataSources.TheFipsterAppPlog;

    public List<FileExtraction> Extract(string filepath, DateTime? _ = null)
    {
        var csv = new CsvFile(filepath, ",");
        var lines = csv.ReadLines().Skip(1);
        var timestamps = lines.Select(ConvertToUtc).ToList();
        var results = timestamps
            .GroupBy(x => x.Date)
            .Select(x => CreateDayWithEvents(filepath, x))
            .ToList();

        return results;
    }

    private FileExtraction CreateDayWithEvents(string filepath, IGrouping<DateTime, DateTime> group)
    {
        var events = group.Select(x => new UnifiedEvent(EventTypes.Plog, x)).ToList();
        var result = new FileExtraction(Source, filepath, group.First().Date, DataKind.Day, events);
        return result;
    }

    private DateTime ConvertToUtc(string[] line)
    {
        var date = line[0];
        var time = line[1];

        var dateParts = date.Split(".").Select(int.Parse).ToArray();
        var timeParts = time.Split(":").Select(int.Parse).ToArray();

        var timestamp = DateHelper.GetUtcDateFromGermanLocalTime(
            dateParts[0],
            dateParts[1],
            dateParts[2],
            timeParts[0],
            timeParts[1]
        );

        return timestamp;
    }
}
