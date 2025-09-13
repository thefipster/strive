using Fipster.TrackMe.Domain;
using Fipster.TrackMe.Domain.Formats;
using Fipster.TrackMe.Domain.Models;
using Fipster.TrackMe.Domain.Tools;
using Fipster.TrackMe.Importer.Modules.Abstractions;

namespace Fipster.TrackMe.Importer.Modules.RunGps;

public class RunGpsCsvImporter : IFileImporter
{
    public const string Type = "rungps_csv";
    public DataSources Source => DataSources.RunGpsCsvV1;

    private List<string> Header =
    [
        "type;latitude;longitude;speed;heading;height;distance;timestamp;",
        "type;latitude;longitude;speed;heading;height;distance;timestamp;heartRate;gpsDilution;temperature;cadence;",
    ];

    public ImportClassification? Classify(string filepath)
    {
        var peeker = new FilePeeker(filepath);
        var lines = peeker.ReadLines(2);

        if (lines == null || lines.Count() != 2)
            return null;

        if (Header.All(x => x != lines.First()))
            return null;

        var data = lines.Last().Split(";", StringSplitOptions.RemoveEmptyEntries);
        var date = DateTime.Parse(data[7]);

        return new ImportClassification
        {
            Filepath = filepath,
            Source = Source,
            Filetype = Type,
            Datetime = date,
            Datetype = DateRanges.Day,
        };
    }

    public List<FileExtraction> Extract(ArchiveIndex file)
    {
        var csv = new CsvFile(file.Filepath, ";");
        var lines = csv.ReadLines();
        var start = lines.First(x => x[0] == "TrainingStart");
        var end = lines.Last(x => x[0] == "TrainingStop");
        var positions = lines.Where(x => x[0] == "Position");
        var gps = new List<GpsPoint>();

        var startDate = DateTime.Parse(start[7]);
        var endDate = DateTime.Parse(end[7]);
        var duration = (int)(endDate - startDate).TotalSeconds;

        var attributes = FileExtraction.EmptyAttributes;
        attributes.Add(Parameters.StartTime, startDate.ToString("s"));
        attributes.Add(Parameters.Duration, duration.ToString());
        attributes.Add(Parameters.Distance, end[6]);

        var durationSeries = new List<string>();
        var latSeries = new List<string>();
        var lonSeries = new List<string>();
        var speedSeries = new List<string>();
        var headingSeries = new List<string>();
        var altitudeSeries = new List<string>();
        var distanceSeries = new List<string>();

        foreach (var position in positions)
        {
            latSeries.Add(position[1]);
            lonSeries.Add(position[2]);
            speedSeries.Add(position[3]);
            headingSeries.Add(position[4]);
            altitudeSeries.Add(position[5]);
            distanceSeries.Add(position[6]);

            var timestamp = DateTime.Parse(position[7]);
            var seconds = (int)(timestamp - startDate).TotalSeconds;
            durationSeries.Add(seconds.ToString());
        }

        var series = FileExtraction.EmptySeries;
        series.Add(Parameters.Duration, durationSeries);
        series.Add(Parameters.Latitude, latSeries);
        series.Add(Parameters.Longitude, lonSeries);
        series.Add(Parameters.Speed, speedSeries);
        series.Add(Parameters.Heading, headingSeries);
        series.Add(Parameters.Altitude, altitudeSeries);
        series.Add(Parameters.Distance, distanceSeries);

        var result = new FileExtraction(
            Source,
            file.Filepath,
            startDate,
            DateRanges.Time,
            attributes,
            series
        );
        return [result];
    }
}
