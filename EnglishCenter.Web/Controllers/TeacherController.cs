using System.Globalization;
using EnglishCenter.Web.Models.Teacher;
using EnglishCenter.Web.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Web.Controllers;

[Authorize(Roles = "Teacher")]
public sealed class TeacherController : Controller
{
    private readonly ITeacherApiClient _teacherApiClient;
    private readonly ILogger<TeacherController> _logger;

    public TeacherController(ITeacherApiClient teacherApiClient, ILogger<TeacherController> logger)
    {
        _teacherApiClient = teacherApiClient;
        _logger = logger;
    }

    // GET: Teacher
    [HttpGet]
    public IActionResult Index()
    {
        return RedirectToAction(nameof(Classes));
    }

    // GET: Teacher/Classes
    [HttpGet]
    public async Task<IActionResult> Classes(CancellationToken cancellationToken)
    {
        try
        {
            return View(new TeacherClassesPageViewModel
            {
                Classes = await _teacherApiClient.GetAssignedClassesAsync(cancellationToken)
            });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to load teacher classes.");
            TempData["ErrorMessage"] = exception.Message;
            return View(new TeacherClassesPageViewModel());
        }
    }

    // GET: Teacher/Schedule
    [HttpGet]
    public async Task<IActionResult> Schedule(int? month, int? year, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var resolvedMonth = month ?? today.Month;
        var resolvedYear = year ?? today.Year;

        try
        {
            var schedules = await _teacherApiClient.GetScheduleAsync(null, null, resolvedMonth, resolvedYear, cancellationToken);
            return View(BuildSchedulePageViewModel(resolvedMonth, resolvedYear, schedules));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to load teacher schedule.");
            TempData["ErrorMessage"] = exception.Message;
            return View(BuildSchedulePageViewModel(resolvedMonth, resolvedYear, []));
        }
    }

    // GET: Teacher/Students?classId={classId}
    [HttpGet]
    public async Task<IActionResult> Students(int classId, CancellationToken cancellationToken)
    {
        try
        {
            var classes = await _teacherApiClient.GetAssignedClassesAsync(cancellationToken);
            var classItem = classes.FirstOrDefault(item => item.ClassId == classId);
            if (classItem is null)
            {
                TempData["ErrorMessage"] = "Class was not found.";
                return RedirectToAction(nameof(Classes));
            }

            var students = await _teacherApiClient.GetStudentsByClassAsync(classId, cancellationToken);
            if (students is null)
            {
                TempData["ErrorMessage"] = "Class was not found.";
                return RedirectToAction(nameof(Classes));
            }

            return View(new TeacherStudentsPageViewModel
            {
                ClassId = classId,
                ClassName = classItem.ClassName,
                Students = students
            });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to load students for class {ClassId}.", classId);
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToAction(nameof(Classes));
        }
    }

    // GET: Teacher/Attendance?classId={classId}
    [HttpGet]
    public async Task<IActionResult> Attendance(int classId, string? attendanceDate, int? scheduleId, CancellationToken cancellationToken)
    {
        var resolvedDate = ResolveAttendanceDate(attendanceDate);

        try
        {
            var classItem = await GetAssignedClassAsync(classId, cancellationToken);
            if (classItem is null)
            {
                TempData["ErrorMessage"] = "Class was not found.";
                return RedirectToAction(nameof(Classes));
            }

            var slots = await _teacherApiClient.GetAttendanceSlotsAsync(classId, resolvedDate, cancellationToken);
            if (slots is null)
            {
                TempData["ErrorMessage"] = "Class was not found.";
                return RedirectToAction(nameof(Classes));
            }

            var selectedScheduleId = scheduleId ?? slots.FirstOrDefault()?.ScheduleId;
            var selectedSlot = selectedScheduleId.HasValue
                ? slots.FirstOrDefault(item => item.ScheduleId == selectedScheduleId.Value)
                : null;
            var records = selectedSlot is null
                ? []
                : await _teacherApiClient.GetAttendanceByDateAsync(classId, resolvedDate, selectedSlot.ScheduleId, cancellationToken) ?? [];

            return View(new TeacherAttendancePageViewModel
            {
                ClassId = classId,
                ClassName = classItem.ClassName,
                SelectedScheduleId = selectedSlot?.ScheduleId,
                IsCheckedIn = selectedSlot?.IsCheckedIn == true,
                CheckedInAt = selectedSlot?.CheckedInAt,
                Slots = slots,
                Form = new TeacherAttendanceForm
                {
                    AttendanceDate = resolvedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    ScheduleId = selectedSlot?.ScheduleId ?? 0,
                    Records = records.Select(item => new TeacherAttendanceRecordForm
                    {
                        StudentId = item.StudentId,
                        StudentName = item.StudentName,
                        Status = string.IsNullOrWhiteSpace(item.Status) ? "Present" : item.Status,
                        Note = item.Note
                    }).ToList()
                }
            });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to load attendance for class {ClassId}.", classId);
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToAction(nameof(Classes));
        }
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckInAttendance(int classId, [Bind(Prefix = "Form")] TeacherAttendanceForm form, CancellationToken cancellationToken)
    {
        try
        {
            var attendanceDate = ResolveAttendanceDate(form.AttendanceDate);
            if (form.ScheduleId <= 0)
            {
                throw new ArgumentException("Please select a slot before checking in.");
            }

            var slot = await _teacherApiClient.CheckInAttendanceSlotAsync(classId, new TeacherAttendanceCheckInRequestModel
            {
                AttendanceDate = attendanceDate,
                ScheduleId = form.ScheduleId
            }, cancellationToken);

            TempData[slot is null ? "ErrorMessage" : "SuccessMessage"] = slot is null
                ? "Class or slot was not found."
                : "Check-in successful. You can now take attendance for this slot.";
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to check in for class {ClassId}.", classId);
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Attendance), new { classId, attendanceDate = form.AttendanceDate, scheduleId = form.ScheduleId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveAttendance(int classId, [Bind(Prefix = "Form")] TeacherAttendanceForm form, CancellationToken cancellationToken)
    {
        try
        {
            var attendanceDate = ResolveAttendanceDate(form.AttendanceDate);
            var records = form.Records.Select(item => new UpsertAttendanceItemRequestModel
            {
                StudentId = item.StudentId,
                Status = string.IsNullOrWhiteSpace(item.Status) ? "Present" : item.Status.Trim(),
                Note = item.Note
            }).ToList();

            var result = await _teacherApiClient.UpsertAttendanceAsync(classId, new UpsertAttendanceRequestModel
            {
                AttendanceDate = attendanceDate,
                ScheduleId = form.ScheduleId,
                Records = records
            }, cancellationToken);

            TempData[result is null ? "ErrorMessage" : "SuccessMessage"] = result is null ? "Class was not found." : "Attendance saved.";
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to save attendance for class {ClassId}.", classId);
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Attendance), new { classId, attendanceDate = form.AttendanceDate, scheduleId = form.ScheduleId });
    }

    // GET: Teacher/AttendanceSummary?classId={classId}
    [HttpGet]
    public async Task<IActionResult> AttendanceSummary(int classId, CancellationToken cancellationToken)
    {
        try
        {
            var summary = await _teacherApiClient.GetAttendanceSummaryAsync(classId, cancellationToken);
            if (summary is null)
            {
                TempData["ErrorMessage"] = "Class was not found.";
                return RedirectToAction(nameof(Classes));
            }

            return View(new TeacherAttendanceSummaryPageViewModel { Summary = summary });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to load attendance summary for class {ClassId}.", classId);
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToAction(nameof(Classes));
        }
    }

    // GET: Teacher/GradeComponents?classId={classId}
    [HttpGet]
    public async Task<IActionResult> GradeComponents(int classId, CancellationToken cancellationToken)
    {
        try
        {
            var classItem = await GetAssignedClassAsync(classId, cancellationToken);
            if (classItem is null)
            {
                TempData["ErrorMessage"] = "Class was not found.";
                return RedirectToAction(nameof(Classes));
            }

            var components = await _teacherApiClient.GetGradeComponentsAsync(classId, cancellationToken);
            if (components is null)
            {
                TempData["ErrorMessage"] = "Class was not found.";
                return RedirectToAction(nameof(Classes));
            }

            return View(new TeacherGradeComponentsPageViewModel
            {
                ClassId = classId,
                ClassName = classItem.ClassName,
                CanManageComponents = classItem.EffectiveCanManageGradeComponents,
                Components = components
            });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to load grade components for class {ClassId}.", classId);
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToAction(nameof(Classes));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateGradeComponent(int classId, [Bind(Prefix = "CreateForm")] TeacherGradeComponentForm form, CancellationToken cancellationToken)
    {
        try
        {
            var component = await _teacherApiClient.CreateGradeComponentAsync(classId, new UpsertGradeComponentRequestModel
            {
                ComponentName = form.ComponentName,
                Weight = form.Weight
            }, cancellationToken);

            TempData[component is null ? "ErrorMessage" : "SuccessMessage"] = component is null ? "Class was not found." : "Grade component created.";
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to create grade component for class {ClassId}.", classId);
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(GradeComponents), new { classId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateGradeComponent(int classId, int componentId, TeacherGradeComponentForm form, CancellationToken cancellationToken)
    {
        try
        {
            var component = await _teacherApiClient.UpdateGradeComponentAsync(classId, componentId, new UpsertGradeComponentRequestModel
            {
                ComponentName = form.ComponentName,
                Weight = form.Weight
            }, cancellationToken);

            TempData[component is null ? "ErrorMessage" : "SuccessMessage"] = component is null ? "Component was not found." : "Grade component updated.";
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to update grade component {ComponentId} for class {ClassId}.", componentId, classId);
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(GradeComponents), new { classId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteGradeComponent(int classId, int componentId, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _teacherApiClient.DeleteGradeComponentAsync(classId, componentId, cancellationToken);
            TempData[deleted ? "SuccessMessage" : "ErrorMessage"] = deleted ? "Grade component deleted." : "Component was not found.";
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to delete grade component {ComponentId} for class {ClassId}.", componentId, classId);
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(GradeComponents), new { classId });
    }

    // GET: Teacher/Grades?classId={classId}&componentId={componentId}
    [HttpGet]
    public async Task<IActionResult> Grades(int classId, int componentId, CancellationToken cancellationToken)
    {
        try
        {
            var classItem = await GetAssignedClassAsync(classId, cancellationToken);
            if (classItem is null)
            {
                TempData["ErrorMessage"] = "Class was not found.";
                return RedirectToAction(nameof(Classes));
            }

            var grades = await _teacherApiClient.GetGradesAsync(classId, componentId, cancellationToken);
            if (grades is null)
            {
                TempData["ErrorMessage"] = "Grade component was not found.";
                return RedirectToAction(nameof(GradeComponents), new { classId });
            }

            var componentName = grades.FirstOrDefault()?.ComponentName ?? $"Component #{componentId}";
            return View(new TeacherGradesPageViewModel
            {
                ClassId = classId,
                ClassName = classItem.ClassName,
                ComponentId = componentId,
                ComponentName = componentName,
                Form = new TeacherGradesForm
                {
                    Entries = grades.Select(item => new TeacherGradeEntryForm
                    {
                        StudentId = item.StudentId,
                        StudentName = item.StudentName,
                        GradeValue = item.GradeValue,
                        TeacherComment = item.TeacherComment,
                        UpdatedAt = item.UpdatedAt
                    }).ToList()
                }
            });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to load grades for class {ClassId} component {ComponentId}.", classId, componentId);
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToAction(nameof(GradeComponents), new { classId });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveGrades(int classId, int componentId, [Bind(Prefix = "Form")] TeacherGradesForm form, CancellationToken cancellationToken)
    {
        try
        {
            foreach (var entry in form.Entries)
            {
                await _teacherApiClient.UpsertGradeAsync(classId, componentId, new UpsertGradeRequestModel
                {
                    StudentId = entry.StudentId,
                    GradeValue = entry.GradeValue,
                    TeacherComment = entry.TeacherComment
                }, cancellationToken);
            }

            TempData["SuccessMessage"] = "Grades saved.";
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to save grades for class {ClassId} component {ComponentId}.", classId, componentId);
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Grades), new { classId, componentId });
    }

    // GET: Teacher/Applications
    [HttpGet]
    public async Task<IActionResult> Applications(CancellationToken cancellationToken)
    {
        try
        {
            return View(new TeacherApplicationsPageViewModel
            {
                CreateForm = new TeacherApplicationForm { Type = "Leave" },
                Applications = await _teacherApiClient.GetApplicationsAsync(cancellationToken)
            });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to load teacher applications.");
            TempData["ErrorMessage"] = exception.Message;
            return View(new TeacherApplicationsPageViewModel { CreateForm = new TeacherApplicationForm { Type = "Leave" } });
        }
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateApplication([Bind(Prefix = "CreateForm")] TeacherApplicationForm form, CancellationToken cancellationToken)
    {
        try
        {
            await _teacherApiClient.CreateApplicationAsync(new CreateTeacherApplicationRequestModel
            {
                Title = form.Title,
                Type = form.Type,
                Content = form.Content
            }, cancellationToken);

            TempData["SuccessMessage"] = "Application submitted.";
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to create teacher application.");
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Applications));
    }

    private async Task<TeacherAssignedClassItem?> GetAssignedClassAsync(int classId, CancellationToken cancellationToken)
    {
        var classes = await _teacherApiClient.GetAssignedClassesAsync(cancellationToken);
        return classes.FirstOrDefault(item => item.ClassId == classId);
    }

    private static TeacherSchedulePageViewModel BuildSchedulePageViewModel(
        int month,
        int year,
        IReadOnlyCollection<TeacherScheduleItem> schedules)
    {
        var sortedSchedules = schedules
            .OrderBy(item => item.ScheduleDate)
            .ThenBy(item => item.StartTime)
            .ThenBy(item => item.ClassName)
            .ToArray();

        var monthStart = new DateOnly(year, month, 1);
        var previousMonth = monthStart.AddMonths(-1);
        var nextMonth = monthStart.AddMonths(1);
        var datedSchedules = sortedSchedules.Where(item => item.ScheduleDate.HasValue).ToArray();

        return new TeacherSchedulePageViewModel
        {
            SelectedMonth = month,
            SelectedYear = year,
            DisplayMonthYear = monthStart.ToString("MMMM yyyy", CultureInfo.InvariantCulture),
            PreviousMonth = previousMonth.Month,
            PreviousYear = previousMonth.Year,
            NextMonth = nextMonth.Month,
            NextYear = nextMonth.Year,
            TotalSessions = sortedSchedules.Length,
            BusyDayCount = datedSchedules
                .Select(item => item.ScheduleDate!.Value)
                .Distinct()
                .Count(),
            Schedules = sortedSchedules,
            UndatedSchedules = sortedSchedules.Where(item => !item.ScheduleDate.HasValue).ToArray(),
            CalendarWeeks = BuildScheduleCalendar(monthStart, datedSchedules)
        };
    }

    private static IReadOnlyCollection<TeacherScheduleCalendarWeekViewModel> BuildScheduleCalendar(
        DateOnly monthStart,
        IReadOnlyCollection<TeacherScheduleItem> datedSchedules)
    {
        var monthEnd = new DateOnly(monthStart.Year, monthStart.Month, DateTime.DaysInMonth(monthStart.Year, monthStart.Month));
        var leadingDays = ((int)monthStart.DayOfWeek + 6) % 7;
        var trailingDays = 6 - (((int)monthEnd.DayOfWeek + 6) % 7);
        var gridStart = monthStart.AddDays(-leadingDays);
        var gridEnd = monthEnd.AddDays(trailingDays);
        var scheduleLookup = datedSchedules
            .GroupBy(item => item.ScheduleDate!.Value)
            .ToDictionary(
                item => item.Key,
                item => (IReadOnlyCollection<TeacherScheduleItem>)item
                    .OrderBy(entry => entry.StartTime)
                    .ThenBy(entry => entry.ClassName)
                    .ToArray());
        var weeks = new List<TeacherScheduleCalendarWeekViewModel>();
        var today = DateOnly.FromDateTime(DateTime.Today);

        for (var cursor = gridStart; cursor <= gridEnd; cursor = cursor.AddDays(7))
        {
            var days = new List<TeacherScheduleCalendarDayViewModel>(7);

            for (var offset = 0; offset < 7; offset++)
            {
                var date = cursor.AddDays(offset);
                days.Add(new TeacherScheduleCalendarDayViewModel
                {
                    Date = date,
                    IsCurrentMonth = date.Month == monthStart.Month && date.Year == monthStart.Year,
                    IsToday = date == today,
                    Entries = scheduleLookup.TryGetValue(date, out var entries) ? entries : []
                });
            }

            weeks.Add(new TeacherScheduleCalendarWeekViewModel { Days = days });
        }

        return weeks;
    }

    private static DateOnly ResolveAttendanceDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return DateOnly.FromDateTime(DateTime.Today);
        }

        if (!DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var attendanceDate))
        {
            throw new ArgumentException("Attendance date is invalid.");
        }

        return attendanceDate;
    }
}
