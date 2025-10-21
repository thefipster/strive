using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Base;

namespace TheFipster.ActivityAggregator.Importer.Features.Classification.Components.Vendors.Withings;

public class WithingsSignalClassifier()
    : CsvClassifier(
        DataSources.WithingsSignal,
        DateRanges.AllTime,
        "date,type,frequency,duration,wearposition,signal,doctor_assessment,doctor_name,checkup_date",
        ",",
        0
    );
