using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Base;

namespace TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Vendors.TheFipsterApp;

public class TheFipsterAppWeightClassifier()
    : JsonClassifier(
        DataSources.TheFipsterAppWeight,
        DateRanges.AllTime,
        ["Weight", "Reason", "Date"],
        "Date"
    );
