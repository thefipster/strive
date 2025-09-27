using System.Globalization;
using System.Text.Json;
using TheFipster.ActivityAggregator.Domain;
using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Domain.Exceptions;
using TheFipster.ActivityAggregator.Domain.Models;
using TheFipster.ActivityAggregator.Domain.Models.Scanner;
using TheFipster.ActivityAggregator.Domain.Tools;
using TheFipster.ActivityAggregator.Importer.Modules.Abstractions;
using TheFipster.ActivityAggregator.Polar.Domain;

namespace TheFipster.ActivityAggregator.Importer.Polar;

public class PolarTakeoutCalendarItemsImporter : IFileImporter
{
    public DataSources Source => DataSources.PolarTakeoutCalendarItems;
    public int ClassifierVersion => 1;
    public int ExtractorVersion => 1;

    private readonly HashSet<string> required =
    [
        "exportVersion",
        "perceivedRecovery",
        "physicalInformations",
    ];

    public ImportClassification Classify(FileProbe probe)
    {
        var props = probe.JsonTags;

        if (props == null)
            throw new ClassificationException(probe.Filepath, Source, "Couldn't find valid json.");

        if (!required.IsSubsetOf(props))
            throw new ClassificationException(
                probe.Filepath,
                Source,
                "Couldn't find required properties."
            );

        return new ImportClassification
        {
            Filepath = probe.Filepath,
            Source = Source,
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
