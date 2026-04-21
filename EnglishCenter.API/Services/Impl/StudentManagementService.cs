using EnglishCenter.API.DTOs.Admin;
using EnglishCenter.API.DTOs.Student;
using EnglishCenter.API.Models;
using EnglishCenter.API.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.API.Services.Impl;

public sealed class StudentManagementService : IStudentManagementService
{
    private readonly EnglishCenterDbContext _dbContext;

    public StudentManagementService(EnglishCenterDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<StudentScheduleItemDto>> GetScheduleAsync(int studentId, DateOnly? fromDate, DateOnly? toDate, int? month, int? year, CancellationToken cancellationToken = default)
    {
        await EnsureStudentAsync(studentId, cancellationToken);

        var dateWindow = ResolveDateWindow(fromDate, toDate, month, year);

        var enrolledClassIds = await _dbContext.ClassStudents.AsNoTracking()
            .Where(item => item.StudentId == studentId)
            .Select(item => item.ClassId)
            .ToListAsync(cancellationToken);

        var query = _dbContext.Schedules.AsNoTracking()
            .Include(item => item.Class)
                .ThenInclude(item => item.Course)
            .Where(item => enrolledClassIds.Contains(item.ClassId))
            .AsQueryable();

        if (dateWindow.FromDate.HasValue)
        {
            query = query.Where(item => !item.ScheduleDate.HasValue || item.ScheduleDate.Value >= dateWindow.FromDate.Value);
        }

        if (dateWindow.ToDate.HasValue)
        {
            query = query.Where(item => !item.ScheduleDate.HasValue || item.ScheduleDate.Value <= dateWindow.ToDate.Value);
        }

        return await query
            .OrderBy(item => item.ScheduleDate)
            .ThenBy(item => item.DayOfWeek)
            .ThenBy(item => item.StartTime)
            .ThenBy(item => item.Class.ClassName)
            .Select(item => new StudentScheduleItemDto
            {
                ScheduleId = item.ScheduleId,
                ClassId = item.ClassId,
                ClassName = item.Class.ClassName,
                CourseId = item.Class.CourseId,
                CourseName = item.Class.Course.CourseName,
                ClassStartDate = item.Class.StartDate,
                ClassEndDate = item.Class.EndDate,
                ScheduleDate = item.ScheduleDate,
                DayOfWeek = item.DayOfWeek,
                StartTime = item.StartTime,
                EndTime = item.EndTime,
                Room = item.Room
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ApplicationDto>> GetApplicationsAsync(int studentId, CancellationToken cancellationToken = default)
    {
        var student = await EnsureStudentAsync(studentId, cancellationToken);

        return await _dbContext.Applications.AsNoTracking()
            .Where(item => item.SenderId == studentId)
            .OrderByDescending(item => item.CreatedAt)
            .Select(item => new ApplicationDto
            {
                AppId = item.AppId,
                SenderId = item.SenderId,
                SenderName = student.Fullname,
                SenderRole = student.Role ?? string.Empty,
                Title = item.Title,
                Content = item.Content,
                Type = item.Type,
                Status = item.Status,
                AdminResponse = item.AdminResponse,
                CreatedAt = item.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ApplicationDto> CreateApplicationAsync(int studentId, CreateStudentApplicationRequest request, CancellationToken cancellationToken = default)
    {
        var student = await EnsureStudentAsync(studentId, cancellationToken);

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArgumentException("Application title is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Type))
        {
            throw new ArgumentException("Application type is required.", nameof(request));
        }

        var application = new Application
        {
            SenderId = studentId,
            Title = request.Title.Trim(),
            Content = string.IsNullOrWhiteSpace(request.Content) ? null : request.Content.Trim(),
            Type = request.Type.Trim(),
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            Sender = student
        };

        _dbContext.Applications.Add(application);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApplicationDto
        {
            AppId = application.AppId,
            SenderId = application.SenderId,
            SenderName = student.Fullname,
            SenderRole = student.Role ?? string.Empty,
            Title = application.Title,
            Content = application.Content,
            Type = application.Type,
            Status = application.Status,
            AdminResponse = application.AdminResponse,
            CreatedAt = application.CreatedAt
        };
    }

    public async Task<IReadOnlyCollection<StudentClassDto>> GetClassesAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await EnsureStudentAsync(studentId, cancellationToken);

        return await _dbContext.ClassStudents.AsNoTracking()
            .Where(item => item.StudentId == studentId)
            .OrderBy(item => item.Class.ClassName)
            .Select(item => new StudentClassDto
            {
                ClassId = item.ClassId,
                ClassName = item.Class.ClassName,
                CourseId = item.Class.CourseId,
                CourseName = item.Class.Course.CourseName,
                TeacherName = item.Class.Teacher != null ? item.Class.Teacher.Fullname : null,
                StartDate = item.Class.StartDate,
                EndDate = item.Class.EndDate,
                Status = item.Class.Status,
                EnrollmentDate = item.EnrollmentDate
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<StudentClassDetailDto?> GetClassDetailAsync(int studentId, int classId, CancellationToken cancellationToken = default)
    {
        await EnsureStudentAsync(studentId, cancellationToken);

        var enrollment = await _dbContext.ClassStudents.AsNoTracking()
            .Where(item => item.StudentId == studentId && item.ClassId == classId)
            .Select(item => new
            {
                item.EnrollmentDate,
                ClassId = item.Class.ClassId,
                ClassName = item.Class.ClassName,
                CourseId = item.Class.CourseId,
                CourseName = item.Class.Course.CourseName,
                TeacherName = item.Class.Teacher != null ? item.Class.Teacher.Fullname : null,
                StartDate = item.Class.StartDate,
                EndDate = item.Class.EndDate,
                Status = item.Class.Status
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (enrollment is null)
        {
            return null;
        }

        var attendance = await _dbContext.Attendances.AsNoTracking()
            .Where(item => item.StudentId == studentId && item.Schedule.ClassId == classId)
            .OrderBy(item => item.AttendanceDate)
            .ThenBy(item => item.Schedule.StartTime)
            .Select(item => new StudentAttendanceItemDto
            {
                AttendanceId = item.AttendanceId,
                ScheduleId = item.ScheduleId,
                AttendanceDate = item.AttendanceDate,
                StartTime = item.Schedule.StartTime,
                EndTime = item.Schedule.EndTime,
                Room = item.Schedule.Room,
                Status = item.Status,
                Note = item.Note
            })
            .ToListAsync(cancellationToken);

        var components = await _dbContext.GradeComponents.AsNoTracking()
            .Where(item => item.ClassId == classId)
            .OrderBy(item => item.ComponentName)
            .Select(item => new { item.ComponentId, item.ComponentName, item.Weight })
            .ToListAsync(cancellationToken);

        var componentIds = components.Select(item => item.ComponentId).ToList();
        var gradesByComponent = await _dbContext.Grades.AsNoTracking()
            .Where(item => item.StudentId == studentId && componentIds.Contains(item.ComponentId))
            .ToDictionaryAsync(item => item.ComponentId, cancellationToken);

        var grades = components.Select(component =>
        {
            gradesByComponent.TryGetValue(component.ComponentId, out var grade);
            return new StudentGradeItemDto
            {
                ComponentId = component.ComponentId,
                ComponentName = component.ComponentName,
                Weight = component.Weight,
                GradeValue = grade?.GradeValue,
                TeacherComment = grade?.TeacherComment
            };
        }).ToList();

        return new StudentClassDetailDto
        {
            ClassId = enrollment.ClassId,
            ClassName = enrollment.ClassName,
            CourseId = enrollment.CourseId,
            CourseName = enrollment.CourseName,
            TeacherName = enrollment.TeacherName,
            StartDate = enrollment.StartDate,
            EndDate = enrollment.EndDate,
            Status = enrollment.Status,
            EnrollmentDate = enrollment.EnrollmentDate,
            Attendance = attendance,
            Grades = grades
        };
    }

    private async Task<User> EnsureStudentAsync(int studentId, CancellationToken cancellationToken)
    {
        var student = await _dbContext.Users.FirstOrDefaultAsync(item => item.UserId == studentId, cancellationToken);
        if (student is null)
        {
            throw new ArgumentException("Student was not found.", nameof(studentId));
        }

        if (!string.Equals(student.Role, "Student", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The specified user is not a student.");
        }

        return student;
    }

    private static (DateOnly? FromDate, DateOnly? ToDate) ResolveDateWindow(DateOnly? fromDate, DateOnly? toDate, int? month, int? year)
    {
        if (month.HasValue || year.HasValue)
        {
            if (!month.HasValue || !year.HasValue)
            {
                throw new ArgumentException("Month and year must be provided together.");
            }

            if (fromDate.HasValue || toDate.HasValue)
            {
                throw new ArgumentException("Use either from/to date or month/year filter, not both.");
            }

            if (month.Value < 1 || month.Value > 12)
            {
                throw new ArgumentException("Month must be between 1 and 12.");
            }

            var firstDay = new DateOnly(year.Value, month.Value, 1);
            return (firstDay, firstDay.AddMonths(1).AddDays(-1));
        }

        if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
        {
            throw new ArgumentException("From date must be earlier than or equal to to date.");
        }

        return (fromDate, toDate);
    }
}
