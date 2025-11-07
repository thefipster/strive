using Fip.Strive.Core.Ingestion.Domain.Enums;
using Fip.Strive.Ingestion.Application.Components;
using Fip.Strive.Ingestion.Domain.Enums;

namespace Fip.Strive.Ingestion.Application.Vendors.Google.Fitbit.Weight;

public class GoogleFitbitWeightClassifier()
    : CsvClassifier(DataSources.GoogleFitbitWeight, DateRanges.Multi, "timestamp,weight grams");
