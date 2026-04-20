namespace EnglishCenter.Web.Models.Teacher;

public sealed class TeacherClassesPageViewModel
{
    public IReadOnlyCollection<TeacherAssignedClassItem> Classes { get; set; } = [];
}

public sealed class TeacherSchedulePageViewModel
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

    public IReadOnlyCollection<TeacherScheduleItem> Schedules { get; set; } = [];

    public IReadOnlyCollection<TeacherScheduleItem> UndatedSchedules { get; set; } = [];

    public IReadOnlyCollection<TeacherScheduleCalendarWeekViewModel> CalendarWeeks { get; set; } = [];
}

public sealed class TeacherScheduleCalendarWeekViewModel
{
    public IReadOnlyCollection<TeacherScheduleCalendarDayViewModel> Days { get; set; } = [];
}

public sealed class TeacherScheduleCalendarDayViewModel
{
    public DateOnly Date { get; set; }

    public bool IsCurrentMonth { get; set; }

    public bool IsToday { get; set; }

    public IReadOnlyCollection<TeacherScheduleItem> Entries { get; set; } = [];
}

public sealed class TeacherStudentsPageViewModel
{
    public int ClassId { get; set; }

    public string ClassName { get; set; } = string.Empty;

    public IReadOnlyCollection<TeacherStudentItem> Students { get; set; } = [];
}

public sealed class TeacherAttendancePageViewModel
{
    public int ClassId { get; set; }

    public string ClassName { get; set; } = string.Empty;

    public TeacherAttendanceForm Form { get; set; } = new();
}

public sealed class TeacherAttendanceForm
{
    public string AttendanceDate { get; set; } = string.Empty;

    public List<TeacherAttendanceRecordForm> Records { get; set; } = [];
}

public sealed class TeacherAttendanceRecordForm
{
    public int StudentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public string Status { get; set; } = "Present";

    public string? Note { get; set; }
}

public sealed class TeacherAttendanceSummaryPageViewModel
{
    public AttendanceSummaryItem Summary { get; set; } = new();
}

public sealed class TeacherGradeComponentsPageViewModel
{
    public int ClassId { get; set; }

    public string ClassName { get; set; } = string.Empty;

    public bool CanManageComponents { get; set; }

    public TeacherGradeComponentForm CreateForm { get; set; } = new();

    public IReadOnlyCollection<TeacherGradeComponentItem> Components { get; set; } = [];
}

public sealed class TeacherGradeComponentForm
{
    public string ComponentName { get; set; } = string.Empty;

    public decimal Weight { get; set; }
}

public sealed class TeacherGradesPageViewModel
{
    public int ClassId { get; set; }

    public string ClassName { get; set; } = string.Empty;

    public int ComponentId { get; set; }

    public string ComponentName { get; set; } = string.Empty;

    public TeacherGradesForm Form { get; set; } = new();
}

public sealed class TeacherGradesForm
{
    public List<TeacherGradeEntryForm> Entries { get; set; } = [];
}

public sealed class TeacherGradeEntryForm
{
    public int StudentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public decimal? GradeValue { get; set; }

    public string? TeacherComment { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

public sealed class TeacherApplicationsPageViewModel
{
    public TeacherApplicationForm CreateForm { get; set; } = new();

    public IReadOnlyCollection<TeacherApplicationItem> Applications { get; set; } = [];
}

public sealed class TeacherApplicationForm
{
    public string Title { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string? Content { get; set; }
}
