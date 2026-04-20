namespace EnglishCenter.API.DTOs;

public class ClassListDto
{
    public int ClassId { get; set; }

    public string ClassName { get; set; } = string.Empty;

    public int CourseId { get; set; }

    public string CourseName { get; set; } = string.Empty;

    public int? TeacherId { get; set; }

    public string? TeacherName { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Status { get; set; }

    public bool AllowTeacherGradeComponentManagement { get; set; }

    public int StudentCount { get; set; }
}

public sealed class ClassDetailDto : ClassListDto
{
    public IReadOnlyCollection<ClassStudentDto> Students { get; set; } = [];

    public IReadOnlyCollection<ScheduleDto> Schedules { get; set; } = [];
}

public sealed class CreateClassRequest
{
    public string ClassName { get; set; } = string.Empty;

    public int CourseId { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Status { get; set; }

    public bool AllowTeacherGradeComponentManagement { get; set; }

    public int? TeacherId { get; set; }
}

public sealed class UpdateClassRequest
{
    public string ClassName { get; set; } = string.Empty;

    public int CourseId { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Status { get; set; }

    public bool? AllowTeacherGradeComponentManagement { get; set; }
}

public sealed class AssignTeacherRequest
{
    public int TeacherId { get; set; }
}

public sealed class AssignStudentsRequest
{
    public IReadOnlyCollection<int> StudentIds { get; set; } = [];
}

public sealed class ClassStudentDto
{
    public int StudentId { get; set; }

    public string Fullname { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string? Email { get; set; }

    public DateTime? EnrollmentDate { get; set; }
}

public sealed class ScheduleDto
{
    public int ScheduleId { get; set; }

    public DateOnly? ScheduleDate { get; set; }

    public int? DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string? Room { get; set; }
}

public sealed class UpsertScheduleRequest
{
    public DateOnly? ScheduleDate { get; set; }

    public int? DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string? Room { get; set; }
}

public sealed class UpdateClassSchedulesRequest
{
    public IReadOnlyCollection<UpsertScheduleRequest> Schedules { get; set; } = [];
}

public sealed class ClassSchedulePlannerDto
{
    public ClassDetailDto Class { get; set; } = new();

    public CourseDto Course { get; set; } = new();

    public DateOnly MonthStart { get; set; }

    public DateOnly MonthEnd { get; set; }

    public int SelectedMonth { get; set; }

    public int SelectedYear { get; set; }

    public int SelectedWeekIndex { get; set; }

    public int PreviousMonth { get; set; }

    public int PreviousYear { get; set; }

    public int NextMonth { get; set; }

    public int NextYear { get; set; }

    public IReadOnlyCollection<ScheduleSlotDefinitionDto> SlotDefinitions { get; set; } = [];

    public IReadOnlyCollection<SchedulePlannerWeekOptionDto> WeekOptions { get; set; } = [];

    public SchedulePlannerWeekDto CurrentWeek { get; set; } = new();

    public int AllowedSelections { get; set; }

    public int CurrentSelections { get; set; }
}

public sealed class SaveClassSchedulePlannerRequest
{
    public IReadOnlyCollection<string> SelectedSlots { get; set; } = [];

    public Dictionary<string, string?> RoomBySlotKey { get; set; } = new();

    public int Month { get; set; }

    public int Year { get; set; }

    public int WeekIndex { get; set; }
}

public sealed class ScheduleSlotDefinitionDto
{
    public int SlotNumber { get; set; }

    public string Label { get; set; } = string.Empty;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }
}

public sealed class SchedulePlannerDayDto
{
    public DateOnly Date { get; set; }

    public bool IsCurrentMonth { get; set; }

    public bool IsInsideClassRange { get; set; }

    public IReadOnlyCollection<SchedulePlannerSlotDto> Slots { get; set; } = [];
}

public sealed class SchedulePlannerWeekDto
{
    public int WeekIndex { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public IReadOnlyCollection<SchedulePlannerDayDto> Days { get; set; } = [];
}

public sealed class SchedulePlannerWeekOptionDto
{
    public int WeekIndex { get; set; }

    public string Label { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }
}

public sealed class SchedulePlannerSlotDto
{
    public string Key { get; set; } = string.Empty;

    public int SlotNumber { get; set; }

    public string Label { get; set; } = string.Empty;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public bool IsSelected { get; set; }

    public string? Room { get; set; }
}

public sealed class UpdateClassGradeComponentPermissionRequest
{
    public bool AllowTeacherGradeComponentManagement { get; set; }
}