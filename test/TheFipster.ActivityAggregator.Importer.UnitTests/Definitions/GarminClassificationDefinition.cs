using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Garmin;
using TheFipster.ActivityAggregator.Importer.UnitTests.Models;

namespace TheFipster.ActivityAggregator.Importer.UnitTests.Definitions;

public class GarminClassificationDefinition
{
    public static IEnumerable<object[]> GetClassificationCases()
    {
        yield return
        [
            new FileClassificationCase(
                typeof(GarminSleepDataImporter),
                Vendors.Garmin,
                "2025-09-24_2025-10-06_137773893_sleepData.json",
                "0001-01-01",
                DateRanges.AllTime,
                DataSources.GarminSleepData
            ),
        ];
        yield return
        [
            new FileClassificationCase(
                typeof(GarminUdsFileImporter),
                Vendors.Garmin,
                "UDSFile_2025-06-27_2025-10-05.json",
                "0001-01-01",
                DateRanges.AllTime,
                DataSources.GarminUdsFile
            ),
        ];
    }
}
