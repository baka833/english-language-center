namespace EnglishCenter.API.DTOs;

public sealed class TeacherAssignedClassDto
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

    public IReadOnlyCollection<ScheduleDto> Schedules { get; set; } = [];
}

public sealed class TeacherScheduleItemDto
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

public sealed class TeacherStudentDto
{
    public int StudentId { get; set; }

    public string Fullname { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string? Email { get; set; }

    public DateOnly? Dob { get; set; }

    public DateTime? EnrollmentDate { get; set; }
}

public sealed class AttendanceRecordDto
{
    public int? AttendanceId { get; set; }

    public int? ScheduleId { get; set; }

    public int StudentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public DateOnly AttendanceDate { get; set; }

    public string? Status { get; set; }

    public string? Note { get; set; }
}

public sealed class UpsertAttendanceItemRequest
{
    public int StudentId { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? Note { get; set; }
}

public sealed class UpsertAttendanceRequest
{
    public DateOnly AttendanceDate { get; set; }

    public int ScheduleId { get; set; }

    public IReadOnlyCollection<UpsertAttendanceItemRequest> Records { get; set; } = [];
}

public sealed class AttendanceSlotDto
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

public sealed class TeacherAttendanceCheckInRequest
{
    public DateOnly AttendanceDate { get; set; }

    public int ScheduleId { get; set; }
}

public sealed class AttendanceSummaryDto
{
    public int ClassId { get; set; }

    public string ClassName { get; set; } = string.Empty;

    public int TotalSessions { get; set; }

    public IReadOnlyCollection<StudentAttendanceSummaryDto> Students { get; set; } = [];
}

public sealed class StudentAttendanceSummaryDto
{
    public int StudentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public int MarkedSessions { get; set; }

    public int PresentCount { get; set; }

    public int AbsentCount { get; set; }

    public int LateCount { get; set; }

    public int ExcusedCount { get; set; }
}

public sealed class TeacherGradeComponentDto
{
    public int ComponentId { get; set; }

    public int ClassId { get; set; }

    public string ComponentName { get; set; } = string.Empty;

    public decimal Weight { get; set; }
}

public sealed class UpsertGradeComponentRequest
{
    public string ComponentName { get; set; } = string.Empty;

    public decimal Weight { get; set; }
}

public sealed class GradeEntryDto
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

public sealed class UpsertGradeRequest
{
    public int StudentId { get; set; }

    public decimal? GradeValue { get; set; }

    public string? TeacherComment { get; set; }
}

public sealed class CreateTeacherApplicationRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    public string Type { get; set; } = string.Empty;
}