using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.Withings.Other;

public class WithingsOtherClassifier()
    : CsvClassifier(DataSources.WithingsOther, DateRanges.Multi, "type,date,value", ",", 1);
