using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Base;

namespace TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Vendors.Withings;

public class WithingsOtherClassifier()
    : CsvClassifier(DataSources.WithingsOther, DateRanges.AllTime, "type,date,value", ",", 1);
