using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.Withings.Signal;

public class WithingsSignalClassifier()
    : CsvClassifier(
        DataSources.WithingsSignal,
        DateRanges.Multi,
        "date,type,frequency,duration,wearposition,signal,doctor_assessment,doctor_name,checkup_date",
        ",",
        0
    );
