namespace EnglishCenter.Web.Models.Teacher;

public sealed class TeacherAssignedClassItem
{
    public int ClassId { get; set; }

    public string ClassName { get; set; } = string.Empty;

    public int CourseId { get; set; }

    public string CourseName { get; set; } = string.Empty;

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Status { get; set; }

    public bool AllowTeacherGradeComponentManagement { get; set; }

    public bool TeacherCanManageGradeComponents { get; set; }

    public bool EffectiveCanManageGradeComponents { get; set; }

    public int StudentCount { get; set; }

    public IReadOnlyCollection<TeacherScheduleSlotItem> Schedules { get; set; } = [];
}

public sealed class TeacherScheduleItem
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

public sealed class TeacherScheduleSlotItem
{
    public int ScheduleId { get; set; }

    public DateOnly? ScheduleDate { get; set; }

    public int? DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string? Room { get; set; }
}

public sealed class TeacherStudentItem
{
    public int StudentId { get; set; }

    public string Fullname { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string? Email { get; set; }

    public DateOnly? Dob { get; set; }

    public DateTime? EnrollmentDate { get; set; }
}

public sealed class AttendanceRecordItem
{
    public int? AttendanceId { get; set; }

    public int ScheduleId { get; set; }

    public int StudentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public DateOnly AttendanceDate { get; set; }

    public string? Status { get; set; }

    public string? Note { get; set; }
}

public sealed class UpsertAttendanceItemRequestModel
{
    public int StudentId { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? Note { get; set; }
}

public sealed class UpsertAttendanceRequestModel
{
    public DateOnly AttendanceDate { get; set; }

    public int ScheduleId { get; set; }

    public IReadOnlyCollection<UpsertAttendanceItemRequestModel> Records { get; set; } = [];
}

public sealed class AttendanceSlotItem
{
    public int ScheduleId { get; set; }

    public DateOnly? ScheduleDate { get; set; }

    public int? DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string? Room { get; set; }

    public bool IsCheckedIn { get; set; }

    public DateTime? CheckedInAt { get; set; }

    public int RecordedStudents { get; set; }
}

public sealed class TeacherAttendanceCheckInRequestModel
{
    public DateOnly AttendanceDate { get; set; }

    public int ScheduleId { get; set; }
}

public sealed class AttendanceSummaryItem
{
    public int ClassId { get; set; }

    public string ClassName { get; set; } = string.Empty;

    public int TotalSessions { get; set; }

    public IReadOnlyCollection<StudentAttendanceSummaryItem> Students { get; set; } = [];
}

public sealed class StudentAttendanceSummaryItem
{
    public int StudentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public int MarkedSessions { get; set; }

    public int PresentCount { get; set; }

    public int AbsentCount { get; set; }

    public int LateCount { get; set; }

    public int ExcusedCount { get; set; }
}

public sealed class TeacherGradeComponentItem
{
    public int ComponentId { get; set; }

    public int ClassId { get; set; }

    public string ComponentName { get; set; } = string.Empty;

    public decimal Weight { get; set; }
}

public sealed class UpsertGradeComponentRequestModel
{
    public string ComponentName { get; set; } = string.Empty;

    public decimal Weight { get; set; }
}

public sealed class GradeEntryItem
{
    public int? GradeId { get; set; }

    public int ComponentId { get; set; }

    public string ComponentName { get; set; } = string.Empty;

    public int StudentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public decimal? GradeValue { get; set; }

    public string? TeacherComment { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

public sealed class UpsertGradeRequestModel
{
    public int StudentId { get; set; }

    public decimal? GradeValue { get; set; }

    public string? TeacherComment { get; set; }
}

public sealed class TeacherApplicationItem
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

public sealed class CreateTeacherApplicationRequestModel
{
    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    public string Type { get; set; } = string.Empty;
}
