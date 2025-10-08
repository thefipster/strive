using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Parser;

namespace TheFipster.ActivityAggregator.Importer.Withings;

public class WithingsSignalImporter()
    : CsvClassifier(
        DataSources.WithingsSignal,
        DateRanges.AllTime,
        "date,type,frequency,duration,wearposition,signal,doctor_assessment,doctor_name,checkup_date",
        ",",
        0
    );
