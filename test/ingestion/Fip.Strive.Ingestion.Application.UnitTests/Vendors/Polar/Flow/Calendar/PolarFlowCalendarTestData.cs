namespace Fip.Strive.Ingestion.Application.UnitTests.Vendors.Polar.Flow.Calendar;

public class PolarFlowCalendarTestData
{
    public const string Extension = ".json";

    public const string File =
        @"{
          ""exportVersion"": ""2.6"",
          ""perceivedRecovery"": [
            {
              ""muscleSoreness"": ""SOME"",
              ""overallFatigue"": ""A_LITTLE"",
              ""sleepUserRating"": ""SLEPT_UNDEFINED"",
              ""sleepUserRatingModified"": ""1970-01-01"",
              ""date"": ""2025-09-13""
            }
          ],
          ""physicalInformations"": [
            {
              ""dateTime"": ""2016-05-08T20:01:22.000"",
              ""height, cm"": 170.0,
              ""weight, kg"": 70.0,
              ""trainingBackground"": ""REGULAR"",
              ""vo2Max"": 50,
              ""maximumHeartRate"": 184,
              ""restingHeartRate"": 55,
              ""aerobicThreshold"": 138,
              ""anaerobicThreshold"": 166,
              ""typicalDay"": ""MOSTLY_SITTING""
            }
          ],
          ""notes"": [
            {
              ""note"": """",
              ""date"": ""2014-10-27""
            }
          ],
          ""feelings"": [
            {
              ""value"": ""NOT_GOOD"",
              ""date"": ""2014-10-27""
            }
          ],
          ""weights"": [
            {
              ""weight"": 75.0,
              ""date"": ""2016-07-09""
            }
          ],
          ""feedbacks"": []
        }";
}
