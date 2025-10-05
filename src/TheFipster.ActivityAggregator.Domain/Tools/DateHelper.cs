namespace TheFipster.ActivityAggregator.Domain.Tools;

public static class DateHelper
{
    public static DateTime GetDateFromMyCollectionDirectory(DirectoryInfo? directory)
    {
        if (directory is not { Exists: true })
            throw new DirectoryNotFoundException(
                $"The directory {directory?.FullName} could not be found."
            );

        var dirName = directory.Name;
        var parts = dirName.Split(" - ");
        var dateString = parts[0].Replace(".", "-");
        var timeString = parts[1];
        return DateTime.Parse(
            $"{dateString} {timeString.Substring(0, 2)}:{timeString.Substring(2, 2)}"
        );
    }

    public static DateTime GetDateFromCsvLine(string line, string separator, int index)
    {
        var cells = line.Split(separator);
        var value = cells[index];
        value = value.Replace("\"", string.Empty);
        return DateTime.Parse(value);
    }

    public static DateTime GetDateFromExtraction(DirectoryInfo directory)
    {
        // ...\pipeline\extractor\2025\08\12\

        var day = directory.Name;
        var month = directory.Parent?.Name;
        var year = directory.Parent?.Parent?.Name;

        if (string.IsNullOrEmpty(month) || string.IsNullOrEmpty(year) || string.IsNullOrEmpty(day))
            throw new ArgumentException(
                $"Directory path {directory.FullName} is not suitable for date conversion."
            );

        return DateTime.Parse($"{year}-{month}-{day}");
    }

    public static DateTime GetDatetimeFromExtraction(DirectoryInfo directory)
    {
        // ...\pipeline\extractor\2025\08\12\14_00

        if (directory.Parent == null)
            throw new ArgumentException(
                $"Directory path {directory.FullName} has no parent directory."
            );

        var time = directory.Name;
        var date = GetDateFromExtraction(directory.Parent);
        if (
            int.TryParse(time.Substring(0, 2), out var hours)
            && int.TryParse(time.Substring(3, 2), out var minutes)
        )
        {
            return date.AddHours(hours).AddMinutes(minutes);
        }

        throw new ArgumentException(
            $"Directory path {directory.FullName} is not suitable for time conversion."
        );
    }

    public static string FsMillisecondFormat => "yyyy-MM-ddTHH-mm-ss-fff";
    public static string MillisecondFormat => "yyyy-MM-ddTHH:mm:ss.fff";
    public static string SecondFormat => "s";
    public static string UiDayFormat => "dd.MM.yyyy";
    public static string UiSecondFormat => "dd.MM.yyyy HH:mm:ss";
    public static string MinuteFormat => "yyyy-MM-ddTHH:mm";
    public static string UiMinuteFormat => "dd.MM.yyyy HH:mm";
    public static string DayFormat => "yyyy-MM-dd";
    public static string MonthFormat => "yyyy-MM";
}
