using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Components;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.Vendors.TheFipsterApp.Plog;

public class TheFipsterAppPlogClassifier()
    : CsvClassifier(DataSources.TheFipsterAppPlog, DateRanges.Multi, "pdate,ptime");
