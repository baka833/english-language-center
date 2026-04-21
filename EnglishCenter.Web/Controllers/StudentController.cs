using System.Globalization;
using System.Security.Claims;
using EnglishCenter.Web.Models.Student;
using EnglishCenter.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Web.Controllers;

[Authorize(Roles = "Student")]
public sealed class StudentController : Controller
{
    private readonly IStudentApiClient _studentApiClient;
    private readonly ILogger<StudentController> _logger;

    public StudentController(IStudentApiClient studentApiClient, ILogger<StudentController> logger)
    {
        _studentApiClient = studentApiClient;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return RedirectToAction(nameof(Schedule));
    }

    [HttpGet]
    public async Task<IActionResult> Schedule(int? month, int? year, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var resolvedMonth = month ?? today.Month;
        var resolvedYear = year ?? today.Year;

        try
        {
            var schedules = await _studentApiClient.GetScheduleAsync(GetCurrentStudentId(), null, null, resolvedMonth, resolvedYear, cancellationToken);
            return View(BuildSchedulePageViewModel(resolvedMonth, resolvedYear, schedules));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to load student schedule.");
            TempData["ErrorMessage"] = exception.Message;
            return View(BuildSchedulePageViewModel(resolvedMonth, resolvedYear, []));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Applications(CancellationToken cancellationToken)
    {
        try
        {
            return View(new StudentApplicationsPageViewModel
            {
                CreateForm = new StudentApplicationForm { Type = "Leave" },
                Applications = await _studentApiClient.GetApplicationsAsync(GetCurrentStudentId(), cancellationToken)
            });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to load student applications.");
            TempData["ErrorMessage"] = exception.Message;
            return View(new StudentApplicationsPageViewModel { CreateForm = new StudentApplicationForm { Type = "Leave" } });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateApplication([Bind(Prefix = "CreateForm")] StudentApplicationForm form, CancellationToken cancellationToken)
    {
        try
        {
            await _studentApiClient.CreateApplicationAsync(GetCurrentStudentId(), new CreateStudentApplicationRequestModel
            {
                Title = form.Title,
                Type = form.Type,
                Content = form.Content
            }, cancellationToken);

            TempData["SuccessMessage"] = "Application submitted.";
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to create student application.");
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Applications));
    }

    private int GetCurrentStudentId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userId, out var studentId))
        {
            throw new InvalidOperationException("Authenticated user id was not found.");
        }

        return studentId;
    }

    private static StudentSchedulePageViewModel BuildSchedulePageViewModel(
        int month,
        int year,
        IReadOnlyCollection<StudentScheduleItem> schedules)
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

        return new StudentSchedulePageViewModel
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

    private static IReadOnlyCollection<StudentScheduleCalendarWeekViewModel> BuildScheduleCalendar(
        DateOnly monthStart,
        IReadOnlyCollection<StudentScheduleItem> datedSchedules)
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
                item => (IReadOnlyCollection<StudentScheduleItem>)item
                    .OrderBy(entry => entry.StartTime)
                    .ThenBy(entry => entry.ClassName)
                    .ToArray());
        var weeks = new List<StudentScheduleCalendarWeekViewModel>();
        var today = DateOnly.FromDateTime(DateTime.Today);

        for (var cursor = gridStart; cursor <= gridEnd; cursor = cursor.AddDays(7))
        {
            var days = new List<StudentScheduleCalendarDayViewModel>(7);

            for (var offset = 0; offset < 7; offset++)
            {
                var date = cursor.AddDays(offset);
                days.Add(new StudentScheduleCalendarDayViewModel
                {
                    Date = date,
                    IsCurrentMonth = date.Month == monthStart.Month && date.Year == monthStart.Year,
                    IsToday = date == today,
                    Entries = scheduleLookup.TryGetValue(date, out var entries) ? entries : []
                });
            }

            weeks.Add(new StudentScheduleCalendarWeekViewModel { Days = days });
        }

        return weeks;
    }
}
