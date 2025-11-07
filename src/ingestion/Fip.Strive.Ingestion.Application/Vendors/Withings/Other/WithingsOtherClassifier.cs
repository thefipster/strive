using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Components;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.Vendors.Withings.Other;

public class WithingsOtherClassifier()
    : CsvClassifier(DataSources.WithingsOther, DateRanges.Multi, "type,date,value", ",", 1);
