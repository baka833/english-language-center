namespace EnglishCenter.Web.Models.Student;

public sealed class StudentSchedulePageViewModel
{
    public int SelectedMonth { get; set; }

    public int SelectedYear { get; set; }

    public string DisplayMonthYear { get; set; } = string.Empty;

    public int PreviousMonth { get; set; }

    public int PreviousYear { get; set; }

    public int NextMonth { get; set; }

    public int NextYear { get; set; }

    public int TotalSessions { get; set; }

    public int BusyDayCount { get; set; }

    public IReadOnlyCollection<StudentScheduleItem> Schedules { get; set; } = [];

    public IReadOnlyCollection<StudentScheduleItem> UndatedSchedules { get; set; } = [];

    public IReadOnlyCollection<StudentScheduleCalendarWeekViewModel> CalendarWeeks { get; set; } = [];
}

public sealed class StudentScheduleCalendarWeekViewModel
{
    public IReadOnlyCollection<StudentScheduleCalendarDayViewModel> Days { get; set; } = [];
}

public sealed class StudentScheduleCalendarDayViewModel
{
    public DateOnly Date { get; set; }

    public bool IsCurrentMonth { get; set; }

    public bool IsToday { get; set; }

    public IReadOnlyCollection<StudentScheduleItem> Entries { get; set; } = [];
}

public sealed class StudentApplicationsPageViewModel
{
    public StudentApplicationForm CreateForm { get; set; } = new();

    public IReadOnlyCollection<StudentApplicationItem> Applications { get; set; } = [];
}

public sealed class StudentApplicationForm
{
    public string Title { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string? Content { get; set; }
}
