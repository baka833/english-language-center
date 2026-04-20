namespace EnglishCenter.Web.Models.Admin;

public class AdminUserSummaryItem
{
    public int UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Fullname { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Role { get; set; }

    public bool CanManageGradeComponents { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }
}

public sealed class AdminUserDetailItem : AdminUserSummaryItem
{
    public DateOnly? Dob { get; set; }

    public string? Gender { get; set; }
}

public sealed class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;

    public string Fullname { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Gender { get; set; }

    public DateOnly? Dob { get; set; }

    public string Role { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public bool CanManageGradeComponents { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class UpdateUserRequest
{
    public string Fullname { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Gender { get; set; }

    public DateOnly? Dob { get; set; }

    public string Role { get; set; } = string.Empty;

    public bool? CanManageGradeComponents { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class LockUserRequest
{
    public bool IsActive { get; set; }
}

public sealed class UpdateTeacherGradeComponentPermissionRequest
{
    public bool CanManageGradeComponents { get; set; }
}

public sealed class ResetPasswordResultItem
{
    public int UserId { get; set; }

    public string NewPassword { get; set; } = string.Empty;
}

public sealed class CourseItem
{
    public int CourseId { get; set; }

    public string CourseName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public int? TotalSlots { get; set; }
}

public sealed class UpsertCourseRequest
{
    public string CourseName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public int? TotalSlots { get; set; }
}

public class ClassListItem
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

public sealed class ClassDetailItem : ClassListItem
{
    public IReadOnlyCollection<ClassStudentItem> Students { get; set; } = [];

    public IReadOnlyCollection<ScheduleItem> Schedules { get; set; } = [];
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

public sealed class ClassStudentItem
{
    public int StudentId { get; set; }

    public string Fullname { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string? Email { get; set; }

    public DateTime? EnrollmentDate { get; set; }
}

public sealed class ScheduleItem
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

public sealed class UpdateClassGradeComponentPermissionRequest
{
    public bool AllowTeacherGradeComponentManagement { get; set; }
}

public sealed class ApplicationItem
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

public sealed class RespondApplicationRequest
{
    public string Status { get; set; } = string.Empty;

    public string? AdminResponse { get; set; }
}

public sealed class AdminLookupItem
{
    public int Id { get; set; }

    public string Label { get; set; } = string.Empty;

    public string? Secondary { get; set; }
}