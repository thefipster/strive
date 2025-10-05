using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Parser;

namespace TheFipster.ActivityAggregator.Importer.Withings;

public class WithingsBloodPressureImporter()
    : CsvClassifier(
        DataSources.WithingsBloodPressure,
        DateRanges.AllTime,
        "Date,\"Heart rate\",Systolic,Diastolic,Comments",
        ",",
        0
    );
