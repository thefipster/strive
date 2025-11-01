namespace Fip.Strive.Core.Application.Features.Dates;

public static class DateChecker
{
    public static bool IsValidDate(int year, int month, int day)
    {
        if (year < 1 || year > 9999)
            return false;

        if (month < 1 || month > 12)
            return false;

        if (day < 1 || day > 31)
            return false;

        int daysInMonth = DateTime.DaysInMonth(year, month);
        return day <= daysInMonth;
    }
}
