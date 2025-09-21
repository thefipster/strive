namespace TheFipster.ActivityAggregator.Web.Views
{
    public class CalendarViewmodel
    {
        public CalendarViewmodel()
            : this(DateTime.Today) { }

        public CalendarViewmodel(DateTime date)
        {
            CurrentMonth = date;
            WeekStarts = new List<DateTime>();
            setup();
        }

        public List<DateTime> WeekStarts { get; set; }

        public DateTime CurrentMonth { get; private set; }

        public DateTime? DatePicker
        {
            get => CurrentMonth;
            set
            {
                if (value.HasValue)
                    CurrentMonth = value.Value;
            }
        }

        public void SetMonth(DateTime date)
        {
            CurrentMonth = date;
            setup();
        }

        public void NextMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(1);
            setup();
        }

        public void PreviousMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(-1);
            setup();
        }

        private void setup()
        {
            var firstDayOfMonth = CurrentMonth.AddDays((CurrentMonth.Day - 1) * -1);
            var lastDayOfMonth = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1)
                .AddMonths(1)
                .AddDays(-1);
            var firstDayOfCalendar = firstDayOfMonth;
            while (firstDayOfCalendar.DayOfWeek != DayOfWeek.Monday)
                firstDayOfCalendar = firstDayOfCalendar.AddDays(-1);

            WeekStarts.Clear();
            while (firstDayOfCalendar <= lastDayOfMonth)
            {
                WeekStarts.Add(firstDayOfCalendar);
                firstDayOfCalendar = firstDayOfCalendar.AddDays(7);
            }
        }
    }
}
