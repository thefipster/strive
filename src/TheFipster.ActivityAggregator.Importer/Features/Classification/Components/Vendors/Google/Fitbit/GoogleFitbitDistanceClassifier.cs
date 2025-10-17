using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Base;

namespace TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Vendors.Google.Fitbit;

public class GoogleFitbitDistanceClassifier()
    : CsvClassifier(DataSources.GoogleFitbitDistance, DateRanges.Month, "timestamp,distance");
