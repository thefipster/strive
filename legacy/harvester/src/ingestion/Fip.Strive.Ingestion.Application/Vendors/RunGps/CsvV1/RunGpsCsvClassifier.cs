using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Contracts;
using Fip.Strive.Ingestion.Domain.Components;
using Fip.Strive.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Domain.Exceptions;
using Fip.Strive.Ingestion.Domain.Models;

namespace Fip.Strive.Ingestion.Application.Vendors.RunGps.CsvV1;

public class RunGpsCsvClassifier : IFileClassifier
{
    public DataSources Source => DataSources.RunGpsCsvV1;
    public int ClassifierVersion => 1;

    private readonly List<string> _header =
    [
        "type;latitude;longitude;speed;heading;height;distance;timestamp;",
        "type;latitude;longitude;speed;heading;height;distance;timestamp;heartRate;gpsDilution;temperature;cadence;",
    ];

    public ImportClassification Classify(FileProbe probe)
    {
        var lines = probe.Lines?.Take(2).ToArray();

        if (lines == null)
            throw new ClassificationException(probe.Filepath, Source, "Couldn't get any lines.");

        if (lines.Length != 2)
            throw new ClassificationException(probe.Filepath, Source, "Couldn't get two lines.");

        if (_header.All(x => x != lines.First()))
            throw new ClassificationException(probe.Filepath, Source, "Couldn't match header.");

        var data = lines.Last().Split(";", StringSplitOptions.RemoveEmptyEntries);
        var date = DateTime.Parse(data[7]);

        return new ImportClassification
        {
            Filepath = probe.Filepath,
            Source = Source,
            Datetime = date,
            Datetype = DateRanges.Session,
        };
    }
}
