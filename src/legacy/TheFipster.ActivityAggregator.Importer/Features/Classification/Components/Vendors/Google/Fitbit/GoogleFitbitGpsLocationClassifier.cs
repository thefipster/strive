using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Base;

namespace TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Vendors.Google.Fitbit;

public class GoogleFitbitGpsLocationClassifier()
    : CsvClassifier(
        DataSources.GoogleFitbitGpsLocation,
        DateRanges.Day,
        "timestamp,latitude,longitude,altitude"
    );
