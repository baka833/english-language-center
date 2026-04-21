namespace EnglishCenter.Web.Models.Student;

public sealed class StudentScheduleItem
{
    public int ScheduleId { get; set; }

    public int ClassId { get; set; }

    public string ClassName { get; set; } = string.Empty;

    public int CourseId { get; set; }

    public string CourseName { get; set; } = string.Empty;

    public DateOnly? ClassStartDate { get; set; }

    public DateOnly? ClassEndDate { get; set; }

    public DateOnly? ScheduleDate { get; set; }

    public int? DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string? Room { get; set; }
}

public sealed class StudentApplicationItem
{
    public int AppId { get; set; }

    public int SenderId { get; set; }

    public string SenderName { get; set; } = string.Empty;

    public string SenderRole { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    public string? Type { get; set; }

    public string? Status { get; set; }

    public string? AdminResponse { get; set; }

    public DateTime? CreatedAt { get; set; }
}

public sealed class CreateStudentApplicationRequestModel
{
    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    public string Type { get; set; } = string.Empty;
}

public sealed class StudentClassItem
{
    public int ClassId { get; set; }

    public string ClassName { get; set; } = string.Empty;

    public int CourseId { get; set; }

    public string CourseName { get; set; } = string.Empty;

    public string? TeacherName { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Status { get; set; }

    public DateTime? EnrollmentDate { get; set; }
}

public sealed class StudentClassAttendanceItem
{
    public int AttendanceId { get; set; }

    public int ScheduleId { get; set; }

    public DateOnly AttendanceDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string? Room { get; set; }

    public string? Status { get; set; }

    public string? Note { get; set; }
}

public sealed class StudentClassGradeItem
{
    public int ComponentId { get; set; }

    public string ComponentName { get; set; } = string.Empty;

    public decimal Weight { get; set; }

    public decimal? GradeValue { get; set; }

    public string? TeacherComment { get; set; }
}

public sealed class StudentClassDetailItem
{
    public int ClassId { get; set; }

    public string ClassName { get; set; } = string.Empty;

    public int CourseId { get; set; }

    public string CourseName { get; set; } = string.Empty;

    public string? TeacherName { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Status { get; set; }

    public DateTime? EnrollmentDate { get; set; }

    public IReadOnlyCollection<StudentClassAttendanceItem> Attendance { get; set; } = [];

    public IReadOnlyCollection<StudentClassGradeItem> Grades { get; set; } = [];
}
