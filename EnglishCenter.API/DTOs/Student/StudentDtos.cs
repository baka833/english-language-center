namespace EnglishCenter.API.DTOs.Student;

public sealed class CreateStudentApplicationRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    public string Type { get; set; } = string.Empty;
}

public sealed class StudentScheduleItemDto
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

public sealed class StudentClassDto
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

public sealed class StudentAttendanceItemDto
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

public sealed class StudentGradeItemDto
{
    public int ComponentId { get; set; }

    public string ComponentName { get; set; } = string.Empty;

    public decimal Weight { get; set; }

    public decimal? GradeValue { get; set; }

    public string? TeacherComment { get; set; }
}

public sealed class StudentClassDetailDto
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

    public IReadOnlyCollection<StudentAttendanceItemDto> Attendance { get; set; } = [];

    public IReadOnlyCollection<StudentGradeItemDto> Grades { get; set; } = [];
}
