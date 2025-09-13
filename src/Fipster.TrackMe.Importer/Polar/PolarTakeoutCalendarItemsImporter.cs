using System.Globalization;
using System.Text.Json;
using Fipster.TrackMe.Domain;
using Fipster.TrackMe.Domain.Models;
using Fipster.TrackMe.Domain.Tools;
using Fipster.TrackMe.Importer.Modules.Abstractions;
using Fipster.TrackMe.Polar.Domain;

namespace Fipster.TrackMe.Importer.Polar;

public class PolarTakeoutCalendarItemsImporter : IFileImporter
{
    public string Type => "polar_takeout_calendar_items";

    public DataSources Source => DataSources.PolarTakeoutCalendarItems;

    public ImportClassification? Classify(string filepath)
    {
        var peeker = new FilePeeker(filepath);

        var result = peeker.ReadChars(256);

        if (!result.Contains("\"perceivedRecovery\""))
            return null;

        return new ImportClassification
        {
            Filepath = filepath,
            Source = Source,
            Filetype = Type,
            Datetype = DateRanges.AllTime,
        };
    }

    public List<FileExtraction> Extract(ArchiveIndex file)
    {
        var json = File.ReadAllText(file.Filepath);
        var calendarItems =
            JsonSerializer.Deserialize<PolarTakeoutCalendarItems>(json)
            ?? throw new ArgumentException("Couldn't parse polar takeout sleep score.");

        var result = new List<FileExtraction>();

        foreach (var item in calendarItems.Weights)
        {
            var attributes = FileExtraction.EmptyAttributes;
            attributes.Add(
                Parameters.Bodyweight,
                item.Value.ToString(CultureInfo.InvariantCulture)
            );

            var extraction = new FileExtraction(
                Source,
                file.Filepath,
                item.Date,
                DateRanges.Day,
                attributes
            );
            result.Add(extraction);
        }

        return result;
    }
}
