using System.Globalization;
using EnglishCenter.Web.Models.Admin;
using EnglishCenter.Web.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Web.Controllers;

[Authorize(Roles = "Admin")]
public sealed class AdminController : Controller
{
    private readonly IAdminApiClient _adminApiClient;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IAdminApiClient adminApiClient, ILogger<AdminController> logger)
    {
        _adminApiClient = adminApiClient;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return RedirectToAction(nameof(Users));
    }

    [HttpGet]
    public async Task<IActionResult> Users(string? role, bool? isActive, CancellationToken cancellationToken)
    {
        try
        {
            return View(new AdminUsersPageViewModel
            {
                RoleFilter = role,
                IsActiveFilter = isActive,
                Users = await _adminApiClient.GetUsersAsync(role, isActive, cancellationToken),
                CreateForm = new CreateUserForm
                {
                    IsActive = true,
                    Role = string.IsNullOrWhiteSpace(role) ? "Teacher" : role
                }
            });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to load users page.");
            TempData["ErrorMessage"] = exception.Message;
            return View(new AdminUsersPageViewModel());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser([Bind(Prefix = "CreateForm")] CreateUserForm form, CancellationToken cancellationToken)
    {
        try
        {
            await _adminApiClient.CreateUserAsync(new CreateUserRequest
            {
                Username = form.Username.Trim(),
                Fullname = form.Fullname.Trim(),
                Email = NormalizeOptional(form.Email),
                Gender = NormalizeOptional(form.Gender),
                Dob = ParseDateOrNull(form.Dob, nameof(form.Dob)),
                Role = form.Role.Trim(),
                Password = form.Password,
                CanManageGradeComponents = form.CanManageGradeComponents,
                IsActive = form.IsActive
            }, cancellationToken);

            TempData["SuccessMessage"] = $"Created user {form.Username}.";
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to create user.");
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Users));
    }

    [HttpGet]
    public async Task<IActionResult> EditUser(int id, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _adminApiClient.GetUserByIdAsync(id, cancellationToken);
            if (user is null)
            {
                TempData["ErrorMessage"] = "User was not found.";
                return RedirectToAction(nameof(Users));
            }

            return View("UserEdit", new UserEditPageViewModel
            {
                User = user,
                Form = new UpdateUserForm
                {
                    Fullname = user.Fullname,
                    Email = user.Email,
                    Gender = user.Gender,
                    Dob = ToDateInput(user.Dob),
                    Role = user.Role ?? string.Empty,
                    CanManageGradeComponents = user.CanManageGradeComponents,
                    IsActive = user.IsActive
                }
            });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to load edit user page {UserId}.", id);
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToAction(nameof(Users));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateUser(int id, [Bind(Prefix = "Form")] UpdateUserForm form, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _adminApiClient.UpdateUserAsync(id, new UpdateUserRequest
            {
                Fullname = form.Fullname.Trim(),
                Email = NormalizeOptional(form.Email),
                Gender = NormalizeOptional(form.Gender),
                Dob = ParseDateOrNull(form.Dob, nameof(form.Dob)),
                Role = form.Role.Trim(),
                CanManageGradeComponents = form.CanManageGradeComponents,
                IsActive = form.IsActive
            }, cancellationToken);

            TempData[user is null ? "ErrorMessage" : "SuccessMessage"] = user is null ? "User was not found." : $"Updated user {user.Username}.";
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to update user {UserId}.", id);
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(EditUser), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUserActivation(int id, bool isActive, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _adminApiClient.SetUserActivationAsync(id, !isActive, cancellationToken);
            TempData[user is null ? "ErrorMessage" : "SuccessMessage"] = user is null ? "User was not found." : $"User {user.Username} is now {(user.IsActive ? "active" : "locked")}.";
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to toggle activation for user {UserId}.", id);
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleTeacherGradePermission(int id, bool canManageGradeComponents, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _adminApiClient.SetTeacherGradePermissionAsync(id, !canManageGradeComponents, cancellationToken);
            TempData[user is null ? "ErrorMessage" : "SuccessMessage"] = user is null ? "User was not found." : $"Updated teacher permission for {user.Username}.";
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to toggle teacher permission for user {UserId}.", id);
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(int id, CancellationToken cancellationToken)
    {
        try
        {
            var reset = await _adminApiClient.ResetPasswordAsync(id, cancellationToken);
            TempData[reset is null ? "ErrorMessage" : "SuccessMessage"] = reset is null ? "User was not found." : $"New password for user #{reset.UserId}: {reset.NewPassword}";
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to reset password for user {UserId}.", id);
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Users));
    }

    [HttpGet]
    public async Task<IActionResult> Courses(CancellationToken cancellationToken)
    {
        try
        {
            return View(new CoursesPageViewModel
            {
                Courses = await _adminApiClient.GetCoursesAsync(cancellationToken)
            });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to load courses page.");
            TempData["ErrorMessage"] = exception.Message;
            return View(new CoursesPageViewModel());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCourse([Bind(Prefix = "CreateForm")] CreateCourseForm form, CancellationToken cancellationToken)
    {
        try
        {
            await _adminApiClient.CreateCourseAsync(new UpsertCourseRequest
            {
                CourseName = form.CourseName.Trim(),
                Description = NormalizeOptional(form.Description),
                Price = form.Price,
                TotalSlots = form.TotalSlots
            }, cancellationToken);

            TempData["SuccessMessage"] = $"Created course {form.CourseName}.";
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to create course.");
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Courses));
    }

    [HttpGet]
    public async Task<IActionResult> EditCourse(int id, CancellationToken cancellationToken)
    {
        try
        {
            var course = await _adminApiClient.GetCourseByIdAsync(id, cancellationToken);
            if (course is null)
            {
                TempData["ErrorMessage"] = "Course was not found.";
                return RedirectToAction(nameof(Courses));
            }

            return View("CourseEdit", new CourseEditPageViewModel
            {
                Course = course,
                Form = new UpdateCourseForm
                {
                    CourseName = course.CourseName,
                    Description = course.Description,
                    Price = course.Price,
                    TotalSlots = course.TotalSlots
                }
            });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to load course {CourseId}.", id);
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToAction(nameof(Courses));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCourse(int id, [Bind(Prefix = "Form")] UpdateCourseForm form, CancellationToken cancellationToken)
    {
        try
        {
            var course = await _adminApiClient.UpdateCourseAsync(id, new UpsertCourseRequest
            {
                CourseName = form.CourseName.Trim(),
                Description = NormalizeOptional(form.Description),
                Price = form.Price,
                TotalSlots = form.TotalSlots
            }, cancellationToken);

            TempData[course is null ? "ErrorMessage" : "SuccessMessage"] = course is null ? "Course was not found." : $"Updated course {course.CourseName}.";
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to update course {CourseId}.", id);
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(EditCourse), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCourse(int id, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _adminApiClient.DeleteCourseAsync(id, cancellationToken);
            TempData[deleted ? "SuccessMessage" : "ErrorMessage"] = deleted ? "Course deleted." : "Course was not found.";
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to delete course {CourseId}.", id);
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Courses));
    }

    [HttpGet]
    public async Task<IActionResult> Classes(CancellationToken cancellationToken)
    {
        try
        {
            var classesTask = _adminApiClient.GetClassesAsync(cancellationToken);
            var coursesTask = _adminApiClient.GetCoursesAsync(cancellationToken);
            var teachersTask = _adminApiClient.GetUsersAsync("Teacher", true, cancellationToken);
            await Task.WhenAll(classesTask, coursesTask, teachersTask);

            return View(new ClassesPageViewModel
            {
                Classes = classesTask.Result,
                Courses = coursesTask.Result.Select(item => new AdminLookupItem { Id = item.CourseId, Label = item.CourseName, Secondary = item.Description }).ToList(),
                Teachers = teachersTask.Result.Select(item => new AdminLookupItem { Id = item.UserId, Label = item.Fullname, Secondary = item.Username }).ToList(),
                CreateForm = new CreateClassForm { Status = "Opening" }
            });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to load classes page.");
            TempData["ErrorMessage"] = exception.Message;
            return View(new ClassesPageViewModel());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateClass([Bind(Prefix = "CreateForm")] CreateClassForm form, CancellationToken cancellationToken)
    {
        try
        {
            await _adminApiClient.CreateClassAsync(new CreateClassRequest
            {
                ClassName = form.ClassName.Trim(),
                CourseId = form.CourseId,
                TeacherId = form.TeacherId,
                StartDate = ParseDateOrNull(form.StartDate, nameof(form.StartDate)),
                EndDate = ParseDateOrNull(form.EndDate, nameof(form.EndDate)),
                Status = NormalizeOptional(form.Status),
                AllowTeacherGradeComponentManagement = form.AllowTeacherGradeComponentManagement
            }, cancellationToken);

            TempData["SuccessMessage"] = $"Created class {form.ClassName}.";
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to create class.");
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Classes));
    }

    [HttpGet]
    public async Task<IActionResult> EditClass(int id, CancellationToken cancellationToken)
    {
        try
        {
            var classTask = _adminApiClient.GetClassByIdAsync(id, cancellationToken);
            var coursesTask = _adminApiClient.GetCoursesAsync(cancellationToken);
            var teachersTask = _adminApiClient.GetUsersAsync("Teacher", true, cancellationToken);
            var studentsTask = _adminApiClient.GetUsersAsync("Student", true, cancellationToken);
            await Task.WhenAll(classTask, coursesTask, teachersTask, studentsTask);

            var classDetail = classTask.Result;
            if (classDetail is null)
            {
                TempData["ErrorMessage"] = "Class was not found.";
                return RedirectToAction(nameof(Classes));
            }

            return View("ClassEdit", new ClassEditPageViewModel
            {
                Class = classDetail,
                Form = new UpdateClassForm
                {
                    ClassName = classDetail.ClassName,
                    CourseId = classDetail.CourseId,
                    TeacherId = classDetail.TeacherId,
                    StartDate = ToDateInput(classDetail.StartDate),
                    EndDate = ToDateInput(classDetail.EndDate),
                    Status = classDetail.Status,
                    AllowTeacherGradeComponentManagement = classDetail.AllowTeacherGradeComponentManagement
                },
                Courses = coursesTask.Result.Select(item => new AdminLookupItem { Id = item.CourseId, Label = item.CourseName, Secondary = item.Description }).ToList(),
                Teachers = teachersTask.Result.Select(item => new AdminLookupItem { Id = item.UserId, Label = item.Fullname, Secondary = item.Username }).ToList(),
                AvailableStudents = studentsTask.Result
                    .Where(item => classDetail.Students.All(student => student.StudentId != item.UserId))
                    .Select(item => new AdminLookupItem { Id = item.UserId, Label = item.Fullname, Secondary = item.Username })
                    .ToList(),
                ScheduleLines = BuildScheduleLines(classDetail.Schedules)
            });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to load class {ClassId}.", id);
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToAction(nameof(Classes));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateClass(int id, [Bind(Prefix = "Form")] UpdateClassForm form, CancellationToken cancellationToken)
    {
        try
        {
            var classDetail = await _adminApiClient.UpdateClassAsync(id, new UpdateClassRequest
            {
                ClassName = form.ClassName.Trim(),
                CourseId = form.CourseId,
                StartDate = ParseDateOrNull(form.StartDate, nameof(form.StartDate)),
                EndDate = ParseDateOrNull(form.EndDate, nameof(form.EndDate)),
                Status = NormalizeOptional(form.Status),
                AllowTeacherGradeComponentManagement = form.AllowTeacherGradeComponentManagement
            }, cancellationToken);

            TempData[classDetail is null ? "ErrorMessage" : "SuccessMessage"] = classDetail is null ? "Class was not found." : $"Updated class {classDetail.ClassName}.";
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to update class {ClassId}.", id);
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(EditClass), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignTeacher(int id, int teacherId, CancellationToken cancellationToken)
    {
        try
        {
            var classDetail = await _adminApiClient.AssignTeacherAsync(id, teacherId, cancellationToken);
            TempData[classDetail is null ? "ErrorMessage" : "SuccessMessage"] = classDetail is null ? "Class was not found." : $"Assigned teacher to {classDetail.ClassName}.";
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to assign teacher for class {ClassId}.", id);
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(EditClass), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleClassGradePermission(int id, bool allowTeacherGradeComponentManagement, CancellationToken cancellationToken)
    {
        try
        {
            var classDetail = await _adminApiClient.SetClassGradePermissionAsync(id, !allowTeacherGradeComponentManagement, cancellationToken);
            TempData[classDetail is null ? "ErrorMessage" : "SuccessMessage"] = classDetail is null ? "Class was not found." : "Updated class permission.";
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to toggle class permission for {ClassId}.", id);
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(EditClass), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignStudents(int id, [FromForm] AssignStudentsForm form, CancellationToken cancellationToken)
    {
        try
        {
            var studentIds = (form.SelectedStudentIds ?? []).Distinct().ToList();
            if (studentIds.Count == 0)
            {
                throw new ArgumentException("Select at least one student to assign.");
            }

            var students = await _adminApiClient.AssignStudentsAsync(id, studentIds, cancellationToken);
            TempData[students is null ? "ErrorMessage" : "SuccessMessage"] = students is null ? "Class was not found." : $"Assigned {studentIds.Count} student(s).";
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to assign students for class {ClassId}.", id);
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(EditClass), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> ScheduleClass(int id, int? month, int? year, int? weekIndex, CancellationToken cancellationToken)
    {
        try
        {
            var planner = await _adminApiClient.GetSchedulePlannerAsync(id, month, year, weekIndex, cancellationToken);
            if (planner is null)
            {
                TempData["ErrorMessage"] = "Class was not found.";
                return RedirectToAction(nameof(Classes));
            }

            return View(planner);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to load schedule planner for class {ClassId}.", id);
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToAction(nameof(EditClass), new { id });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveClassSchedule(int id, [FromForm] SaveSchedulePlannerForm form, CancellationToken cancellationToken)
    {
        try
        {
            form.SelectedSlots ??= [];
            form.RoomBySlotKey ??= new Dictionary<string, string?>();

            var planner = await _adminApiClient.SaveSchedulePlannerAsync(id, form, cancellationToken);
            if (planner is null)
            {
                TempData["ErrorMessage"] = "Class was not found.";
                return RedirectToAction(nameof(Classes));
            }
            TempData["SuccessMessage"] = "Class schedule updated.";
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to save schedule planner for class {ClassId}.", id);
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(ScheduleClass), new { id, month = form.Month, year = form.Year, weekIndex = form.WeekIndex });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveStudent(int id, int studentId, CancellationToken cancellationToken)
    {
        try
        {
            var removed = await _adminApiClient.RemoveStudentAsync(id, studentId, cancellationToken);
            TempData[removed ? "SuccessMessage" : "ErrorMessage"] = removed ? "Removed student from class." : "Student assignment was not found.";
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to remove student {StudentId} from class {ClassId}.", studentId, id);
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(EditClass), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSchedules(int id, UpdateSchedulesForm form, CancellationToken cancellationToken)
    {
        try
        {
            var schedules = ParseSchedules(form.ScheduleLines);
            var result = await _adminApiClient.UpdateSchedulesAsync(id, schedules, cancellationToken);
            TempData[result is null ? "ErrorMessage" : "SuccessMessage"] = result is null ? "Class was not found." : $"Updated {result.Count} schedule slot(s).";
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to update schedules for class {ClassId}.", id);
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(EditClass), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteClass(int id, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _adminApiClient.DeleteClassAsync(id, cancellationToken);
            TempData[deleted ? "SuccessMessage" : "ErrorMessage"] = deleted ? "Class deleted." : "Class was not found.";
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to delete class {ClassId}.", id);
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Classes));
    }

    [HttpGet]
    public async Task<IActionResult> Applications(string? status, CancellationToken cancellationToken)
    {
        try
        {
            return View(new ApplicationsPageViewModel
            {
                StatusFilter = status,
                Applications = await _adminApiClient.GetApplicationsAsync(status, cancellationToken)
            });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to load applications page.");
            TempData["ErrorMessage"] = exception.Message;
            return View(new ApplicationsPageViewModel());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RespondApplication(int id, RespondApplicationForm form, CancellationToken cancellationToken)
    {
        try
        {
            var application = await _adminApiClient.RespondApplicationAsync(id, new RespondApplicationRequest
            {
                Status = form.Status.Trim(),
                AdminResponse = NormalizeOptional(form.AdminResponse)
            }, cancellationToken);

            TempData[application is null ? "ErrorMessage" : "SuccessMessage"] = application is null ? "Application was not found." : $"Responded to application #{application.AppId}.";
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to respond to application {ApplicationId}.", id);
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Applications));
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static DateOnly? ParseDateOrNull(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return date;
        }

        throw new ArgumentException($"{fieldName} is not a valid date.");
    }

    private static string? ToDateInput(DateOnly? value)
    {
        return value?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    private static List<int> ParseStudentIds(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv))
        {
            return [];
        }

        var parsed = new List<int>();
        foreach (var part in csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!int.TryParse(part, out var value))
            {
                throw new ArgumentException($"Student id '{part}' is not valid.");
            }

            parsed.Add(value);
        }

        return parsed.Distinct().ToList();
    }

    private static List<UpsertScheduleRequest> ParseSchedules(string? lines)
    {
        if (string.IsNullOrWhiteSpace(lines))
        {
            return [];
        }

        var schedules = new List<UpsertScheduleRequest>();
        foreach (var line in lines.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = line.Split('|');
            if (parts.Length < 3 || parts.Length > 4)
            {
                throw new ArgumentException("Each schedule line must use the format: day|HH:mm|HH:mm|room.");
            }

            if (!int.TryParse(parts[0].Trim(), out var dayOfWeek))
            {
                throw new ArgumentException($"Invalid day in schedule line '{line}'.");
            }

            if (!TimeOnly.TryParseExact(parts[1].Trim(), "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startTime))
            {
                throw new ArgumentException($"Invalid start time in schedule line '{line}'.");
            }

            if (!TimeOnly.TryParseExact(parts[2].Trim(), "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endTime))
            {
                throw new ArgumentException($"Invalid end time in schedule line '{line}'.");
            }

            schedules.Add(new UpsertScheduleRequest
            {
                ScheduleDate = null,
                DayOfWeek = dayOfWeek,
                StartTime = startTime,
                EndTime = endTime,
                Room = parts.Length == 4 ? NormalizeOptional(parts[3]) : null
            });
        }

        return schedules;
    }

    private static string BuildScheduleLines(IReadOnlyCollection<ScheduleItem> schedules)
    {
        return string.Join(Environment.NewLine, schedules
            .OrderBy(item => item.DayOfWeek)
            .ThenBy(item => item.StartTime)
            .Select(item => $"{item.DayOfWeek}|{item.StartTime:HH\\:mm}|{item.EndTime:HH\\:mm}|{item.Room}"));
    }
}