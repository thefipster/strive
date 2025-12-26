namespace Fip.Strive.Ingestion.Application.UnitTests.Vendors.Polar.Flow.Activity;

public class PolarFlowActivityTestData
{
    public const string Extension = ".json";

    public const string FileVariant1 =
        @"{
          ""exportVersion"": ""2.6"",
          ""date"": ""2014-10-27"",
          ""summary"": {
            ""startTime"": ""08:10:30.000"",
            ""endTime"": ""23:54:00.000"",
            ""stepCount"": 7088,
            ""stepsDistance"": 5233.99755859375,
            ""calories"": 2160,
            ""sleepQuality"": 0.0,
            ""sleepDuration"": ""PT0S"",
            ""inactivityAlertCount"": 0,
            ""dailyMetMinutes"": 197.9624999999992,
            ""activityLevels"": [
              {
                ""level"": ""CONTINUOS_MODERATE"",
                ""duration"": ""PT3240S""
              }
            ]
          },
          ""samples"": {
            ""mets"": [
              {
                ""localTime"": ""08:10:30.000"",
                ""value"": 1.75
              }
            ],
            ""steps"": [
              {
                ""localTime"": ""08:11:30.000"",
                ""value"": 2
              }
            ],
            ""metSources"": [
              """"
            ]
          }
        }";

    public const string FileVariant2 =
        @"{
          ""exportVersion"": ""2.6"",
          ""date"": ""2016-05-09"",
          ""samples"": {
            ""mets"": [
              {
                ""value"": 0.875
              },
              {
                ""value"": 1.2
              }
            ],
            ""steps"": [
              {
                ""value"": 12
              },
              {
                ""value"": 5
              }
            ],
            ""metSources"": [
              """"
            ]
          }
        }";
}
