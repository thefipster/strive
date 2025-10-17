using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Base;

namespace TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Vendors.Google.Fitbit;

public class GoogleFitbitSedentaryPeriodClassifier()
    : CsvClassifier(DataSources.GoogleFitbitSedentaryPeriod, DateRanges.Day, "start time,end time");
