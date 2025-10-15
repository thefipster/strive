using TheFipster.ActivityAggregator.Domain.Enums;
using TheFipster.ActivityAggregator.Importer.Generic;
using TheFipster.ActivityAggregator.Importer.Gpsies;
using TheFipster.ActivityAggregator.Importer.RunGps;
using TheFipster.ActivityAggregator.Importer.UnitTests.Models;

namespace TheFipster.ActivityAggregator.Importer.UnitTests.Definitions;

public static class MyCollectionClassificationDefinitions
{
    public static IEnumerable<object[]> GetClassificationCases()
    {
        yield return
        [
            new FileClassificationCase(
                typeof(GpsiesCsvImporter),
                Vendors.MyCollection,
                "2011.04.03 - 1248\\Rungps03042011150848.csv",
                "2011-04-03 12:48",
                DateRanges.Time,
                DataSources.GpsiesCsv
            ),
        ];

        yield return
        [
            new FileClassificationCase(
                typeof(KmlImporter),
                Vendors.MyCollection,
                "2009.02.09 - 1717\\Gps.kml",
                "2009-02-09 17:17:00",
                DateRanges.Time,
                DataSources.Kml
            ),
        ];

        yield return
        [
            new FileClassificationCase(
                typeof(GpxImporter),
                Vendors.MyCollection,
                "2009.02.09 - 1717\\GpsX.gpx",
                "2009-02-09 17:17:00",
                DateRanges.Time,
                DataSources.Gpx
            ),
        ];

        yield return
        [
            new FileClassificationCase(
                typeof(RunGpsCsvImporter),
                Vendors.MyCollection,
                "2008.03.16 - 1619\\data.csv",
                "2008-03-16 16:20:03.803",
                DateRanges.Time,
                DataSources.RunGpsCsvV1
            ),
        ];

        yield return
        [
            new FileClassificationCase(
                typeof(RunGpsCsv2Importer),
                Vendors.MyCollection,
                "2011.04.03 - 1248\\unbenannt.csv",
                "2011-04-03 12:48:47.401",
                DateRanges.Time,
                DataSources.RunGpsCsvV2
            ),
        ];
    }
}
