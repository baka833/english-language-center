namespace EnglishCenter.Web.Models.Admin;

public sealed class AdminUsersPageViewModel
{
    public string? RoleFilter { get; set; }

    public bool? IsActiveFilter { get; set; }

    public CreateUserForm CreateForm { get; set; } = new();

    public IReadOnlyCollection<AdminUserSummaryItem> Users { get; set; } = [];
}

public sealed class UserEditPageViewModel
{
    public AdminUserDetailItem User { get; set; } = new();

    public UpdateUserForm Form { get; set; } = new();
}

public sealed class CoursesPageViewModel
{
    public CreateCourseForm CreateForm { get; set; } = new();

    public IReadOnlyCollection<CourseItem> Courses { get; set; } = [];
}

public sealed class CourseEditPageViewModel
{
    public CourseItem Course { get; set; } = new();

    public UpdateCourseForm Form { get; set; } = new();
}

public sealed class ClassesPageViewModel
{
    public CreateClassForm CreateForm { get; set; } = new();

    public IReadOnlyCollection<ClassListItem> Classes { get; set; } = [];

    public IReadOnlyCollection<AdminLookupItem> Courses { get; set; } = [];

    public IReadOnlyCollection<AdminLookupItem> Teachers { get; set; } = [];
}

public sealed class ClassEditPageViewModel
{
    public ClassDetailItem Class { get; set; } = new();

    public UpdateClassForm Form { get; set; } = new();

    public IReadOnlyCollection<AdminLookupItem> Courses { get; set; } = [];

    public IReadOnlyCollection<AdminLookupItem> Teachers { get; set; } = [];

    public IReadOnlyCollection<AdminLookupItem> AvailableStudents { get; set; } = [];

    public string ScheduleLines { get; set; } = string.Empty;

    public string StudentIdsCsv { get; set; } = string.Empty;
}

public sealed class ClassSchedulePlannerPageViewModel
{
    public ClassDetailItem Class { get; set; } = new();

    public CourseItem Course { get; set; } = new();

    public DateOnly MonthStart { get; set; }

    public DateOnly MonthEnd { get; set; }

    public int SelectedMonth { get; set; }

    public int SelectedYear { get; set; }

    public int SelectedWeekIndex { get; set; }

    public int PreviousMonth { get; set; }

    public int PreviousYear { get; set; }

    public int NextMonth { get; set; }

    public int NextYear { get; set; }

    public IReadOnlyCollection<ScheduleSlotDefinitionViewModel> SlotDefinitions { get; set; } = [];

    public IReadOnlyCollection<SchedulePlannerWeekOptionViewModel> WeekOptions { get; set; } = [];

    public SchedulePlannerWeekViewModel CurrentWeek { get; set; } = new();

    public int AllowedSelections { get; set; }

    public int CurrentSelections { get; set; }
}

public sealed class ApplicationsPageViewModel
{
    public string? StatusFilter { get; set; }

    public IReadOnlyCollection<ApplicationItem> Applications { get; set; } = [];
}

public sealed class CreateUserForm
{
    public string Username { get; set; } = string.Empty;

    public string Fullname { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Gender { get; set; }

    public string? Dob { get; set; }

    public string Role { get; set; } = "Teacher";

    public string Password { get; set; } = string.Empty;

    public bool CanManageGradeComponents { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class UpdateUserForm
{
    public string Fullname { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Gender { get; set; }

    public string? Dob { get; set; }

    public string Role { get; set; } = string.Empty;

    public bool CanManageGradeComponents { get; set; }

    public bool IsActive { get; set; }
}

public class CreateCourseForm
{
    public string CourseName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public int? TotalSlots { get; set; }
}

public sealed class UpdateCourseForm : CreateCourseForm
{
}

public class CreateClassForm
{
    public string ClassName { get; set; } = string.Empty;

    public int CourseId { get; set; }

    public int? TeacherId { get; set; }

    public string? StartDate { get; set; }

    public string? EndDate { get; set; }

    public string? Status { get; set; } = "Opening";

    public bool AllowTeacherGradeComponentManagement { get; set; }
}

public sealed class UpdateClassForm : CreateClassForm
{
}

public sealed class AssignStudentsForm
{
    public List<int> SelectedStudentIds { get; set; } = [];
}

public sealed class UpdateSchedulesForm
{
    public string ScheduleLines { get; set; } = string.Empty;
}

public sealed class SaveSchedulePlannerForm
{
    public List<string> SelectedSlots { get; set; } = [];

    public Dictionary<string, string?> RoomBySlotKey { get; set; } = new();

    public int Month { get; set; }

    public int Year { get; set; }

    public int WeekIndex { get; set; }
}

public sealed class RespondApplicationForm
{
    public string Status { get; set; } = "Approved";

    public string? AdminResponse { get; set; }
}

public sealed class ScheduleSlotDefinitionViewModel
{
    public int SlotNumber { get; set; }

    public string Label { get; set; } = string.Empty;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }
}

public sealed class SchedulePlannerDayViewModel
{
    public DateOnly Date { get; set; }

    public bool IsCurrentMonth { get; set; }

    public bool IsInsideClassRange { get; set; }

    public IReadOnlyCollection<SchedulePlannerSlotViewModel> Slots { get; set; } = [];
}

public sealed class SchedulePlannerWeekViewModel
{
    public int WeekIndex { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public IReadOnlyCollection<SchedulePlannerDayViewModel> Days { get; set; } = [];
}

public sealed class SchedulePlannerWeekOptionViewModel
{
    public int WeekIndex { get; set; }

    public string Label { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }
}

public sealed class SchedulePlannerSlotViewModel
{
    public string Key { get; set; } = string.Empty;

    public int SlotNumber { get; set; }

    public string Label { get; set; } = string.Empty;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public bool IsSelected { get; set; }

    public string? Room { get; set; }
}