using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Strava;
using TheFipster.ActivityAggregator.Importer.UnitTests.Models;

namespace TheFipster.ActivityAggregator.Importer.UnitTests.Definitions;

public class StravaClassificationDefinition
{
    public static IEnumerable<object[]> GetClassificationCases()
    {
        yield return
        [
            new FileClassificationCase(
                typeof(StravaActivitiesImporter),
                Vendors.Strava,
                "activities.csv",
                "0001-01-01",
                DateRanges.AllTime,
                DataSources.StravaActivities
            ),
        ];
    }
}
