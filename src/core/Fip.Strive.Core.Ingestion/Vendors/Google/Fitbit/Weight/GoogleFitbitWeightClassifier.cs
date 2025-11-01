using Fip.Strive.Core.Domain.Schemas.Ingestion.Enums;
using Fip.Strive.Core.Ingestion.Components;

namespace Fip.Strive.Core.Ingestion.Vendors.Google.Fitbit.Weight;

public class GoogleFitbitWeightClassifier()
    : CsvClassifier(DataSources.GoogleFitbitWeight, DateRanges.Multi, "timestamp,weight grams");
