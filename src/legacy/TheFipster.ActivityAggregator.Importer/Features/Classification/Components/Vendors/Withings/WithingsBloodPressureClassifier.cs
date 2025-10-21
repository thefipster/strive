using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Base;

namespace TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Vendors.Withings;

public class WithingsBloodPressureClassifier()
    : CsvClassifier(
        DataSources.WithingsBloodPressure,
        DateRanges.AllTime,
        "Date,\"Heart rate\",Systolic,Diastolic,Comments",
        ",",
        0
    );
