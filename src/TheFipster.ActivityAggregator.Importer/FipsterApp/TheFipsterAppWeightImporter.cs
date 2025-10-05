using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Parser;

namespace TheFipster.ActivityAggregator.Importer.FipsterApp;

public class TheFipsterAppWeightImporter()
    : JsonClassifier(
        DataSources.TheFipsterAppWeight,
        DateRanges.AllTime,
        ["Weight", "Reason", "Date"],
        "Date"
    );
