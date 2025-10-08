using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Parser;

namespace TheFipster.ActivityAggregator.Importer.Withings;

public class WithingsOtherImporter()
    : CsvClassifier(DataSources.WithingsOther, DateRanges.AllTime, "type,date,value", ",", 1);
