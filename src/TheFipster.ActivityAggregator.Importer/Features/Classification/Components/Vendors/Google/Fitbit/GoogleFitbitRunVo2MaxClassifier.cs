using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Base;

namespace TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Vendors.Google.Fitbit;

public class GoogleFitbitRunVo2MaxClassifier()
    : CsvClassifier(DataSources.GoogleFitbitRunVo2Max, DateRanges.AllTime, "timestamp,run VO2 max");
