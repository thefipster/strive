using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Components;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.Vendors.Withings.Signal;

public class WithingsSignalClassifier()
    : CsvClassifier(
        DataSources.WithingsSignal,
        DateRanges.Multi,
        "date,type,frequency,duration,wearposition,signal,doctor_assessment,doctor_name,checkup_date",
        ",",
        0
    );
