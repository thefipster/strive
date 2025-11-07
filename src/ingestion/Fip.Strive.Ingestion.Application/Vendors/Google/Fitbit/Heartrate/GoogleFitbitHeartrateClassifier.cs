using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Contracts;
using Fip.Strive.Ingestion.Domain.Components;
using Fip.Strive.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Domain.Exceptions;
using Fip.Strive.Ingestion.Domain.Models;

namespace Fip.Strive.Ingestion.Application.Vendors.Google.Fitbit.Heartrate;

public class GoogleFitbitHeartrateClassifier : IFileClassifier
{
    public DataSources Source => DataSources.GoogleFitbitHeartrate;
    public int ClassifierVersion => 1;

    private readonly string _header = "timestamp,beats per minute";

    public ImportClassification Classify(FileProbe probe)
    {
        var lines = probe.Lines?.Take(2).ToArray();

        if (lines == null)
            throw new ClassificationException(probe.Filepath, Source, "Couldn't get any lines.");

        if (lines.Length != 2)
            throw new ClassificationException(probe.Filepath, Source, "Couldn't get two lines.");

        if (_header != lines.First())
            throw new ClassificationException(probe.Filepath, Source, "Couldn't match header.");

        var cells = lines.Last().Split(",");
        var date = DateTime.Parse(cells[0]);

        return new ImportClassification
        {
            Filepath = probe.Filepath,
            Source = Source,
            Datetime = date,
            Datetype = probe.Filepath.Contains("daily") ? DateRanges.Multi : DateRanges.Day,
        };
    }
}
