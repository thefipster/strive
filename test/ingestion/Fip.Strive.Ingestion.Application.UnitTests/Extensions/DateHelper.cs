namespace Fip.Strive.Ingestion.Application.UnitTests.Extensions;

public class DateHelper
{
    public static DateTime ParseDateAsUtc(string dateString)
    {
        return DateTime.Parse(
            dateString,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.AssumeUniversal
                | System.Globalization.DateTimeStyles.AdjustToUniversal
        );
    }
}
