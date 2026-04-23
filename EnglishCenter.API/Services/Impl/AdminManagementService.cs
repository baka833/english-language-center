using System.Globalization;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using EnglishCenter.API.DTOs.Admin;
using EnglishCenter.API.Models;
using EnglishCenter.API.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.API.Services.Impl;

public sealed class AdminManagementService : IAdminManagementService
{
    private static readonly ScheduleSlotDefinitionDto[] FixedSlots =
    [
        new() { SlotNumber = 1, Label = "Slot 1", StartTime = new TimeOnly(7, 30), EndTime = new TimeOnly(9, 30) },
        new() { SlotNumber = 2, Label = "Slot 2", StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(12, 0) },
        new() { SlotNumber = 3, Label = "Slot 3", StartTime = new TimeOnly(13, 0), EndTime = new TimeOnly(15, 0) },
        new() { SlotNumber = 4, Label = "Slot 4", StartTime = new TimeOnly(16, 0), EndTime = new TimeOnly(18, 0) },
        new() { SlotNumber = 5, Label = "Slot 5", StartTime = new TimeOnly(19, 0), EndTime = new TimeOnly(21, 0) }
    ];

    private static readonly HashSet<string> ManageableRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Teacher",
        "Student"
    };

    private static readonly HashSet<string> ApplicationStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Pending",
        "Approved",
        "Rejected"
    };

    private static readonly HashSet<string> ClassStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Opening",
        "Ongoing",
        "Completed"
    };

    private static readonly HashSet<string> AllowedGenders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Male",
        "Female",
        "Other"
    };

    private readonly EnglishCenterDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AdminManagementService(EnglishCenterDbContext dbContext, IPasswordHasher<User> passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<IReadOnlyCollection<UserSummaryDto>> GetUsersAsync(string? role, bool? isActive, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(role))
        {
            query = query.Where(user => user.Role == role);
        }

        if (isActive.HasValue)
        {
            query = query.Where(user => (user.IsActive ?? true) == isActive.Value);
        }

        return await query
            .OrderBy(user => user.Fullname)
            .Select(user => MapUserSummary(user))
            .ToListAsync(cancellationToken);
    }

    public async Task<UserDetailDto?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(item => item.UserId == userId, cancellationToken);
        return user is null ? null : MapUserDetail(user);
    }

    public async Task<UserDetailDto> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        ValidateCreateUserRequest(request);
        ValidateManagedRole(request.Role);

        var username = request.Username.Trim();
        if (await _dbContext.Users.AnyAsync(user => user.Username == username, cancellationToken))
        {
            throw new InvalidOperationException("Username already exists.");
        }

        var user = new User
        {
            Username = username,
            Fullname = request.Fullname.Trim(),
            Email = NormalizeOptional(request.Email),
            Gender = NormalizeOptional(request.Gender),
            Dob = request.Dob,
            Role = NormalizeRole(request.Role),
            CanManageGradeComponents = ResolveTeacherGradeComponentPermission(request.Role, request.CanManageGradeComponents),
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapUserDetail(user);
    }

    public async Task<UserDetailDto?> UpdateUserAsync(int userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        ValidateUpdateUserRequest(request);
        ValidateManagedRole(request.Role);

        var user = await _dbContext.Users.FirstOrDefaultAsync(item => item.UserId == userId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        user.Fullname = request.Fullname.Trim();
        user.Email = NormalizeOptional(request.Email);
        user.Gender = NormalizeOptional(request.Gender);
        user.Dob = request.Dob;
        user.Role = NormalizeRole(request.Role);
        user.CanManageGradeComponents = ResolveTeacherGradeComponentPermission(
            request.Role,
            request.CanManageGradeComponents ?? (user.CanManageGradeComponents ?? false));
        user.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapUserDetail(user);
    }

    public async Task<UserDetailDto?> SetUserActivationAsync(int userId, bool isActive, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(item => item.UserId == userId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        user.IsActive = isActive;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapUserDetail(user);
    }

    public async Task<UserDetailDto?> UpdateTeacherGradeComponentPermissionAsync(int userId, bool canManageGradeComponents, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(item => item.UserId == userId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        if (!string.Equals(user.Role, "Teacher", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Only teachers can receive grade component management permission.");
        }

        user.CanManageGradeComponents = canManageGradeComponents;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapUserDetail(user);
    }

    public async Task<ResetPasswordResultDto?> ResetPasswordAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(item => item.UserId == userId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var newPassword = GeneratePassword();
        user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ResetPasswordResultDto
        {
            UserId = user.UserId,
            NewPassword = newPassword
        };
    }

    public async Task<IReadOnlyCollection<CourseDto>> GetCoursesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Courses.AsNoTracking()
            .OrderBy(course => course.CourseName)
            .Select(course => MapCourse(course))
            .ToListAsync(cancellationToken);
    }

    public async Task<CourseDto?> GetCourseByIdAsync(int courseId, CancellationToken cancellationToken = default)
    {
        var course = await _dbContext.Courses.AsNoTracking().FirstOrDefaultAsync(item => item.CourseId == courseId, cancellationToken);
        return course is null ? null : MapCourse(course);
    }

    public async Task<CourseDto> CreateCourseAsync(UpsertCourseRequest request, CancellationToken cancellationToken = default)
    {
        ValidateCourseRequest(request);

        var normalizedCourseName = request.CourseName.Trim();
        if (await _dbContext.Courses.AnyAsync(item => item.CourseName == normalizedCourseName, cancellationToken))
        {
            throw new InvalidOperationException("Course name already exists.");
        }

        var course = new Course
        {
            CourseName = normalizedCourseName,
            Description = NormalizeOptional(request.Description),
            Price = request.Price,
            TotalSlots = request.TotalSlots
        };

        _dbContext.Courses.Add(course);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapCourse(course);
    }

    public async Task<CourseDto?> UpdateCourseAsync(int courseId, UpsertCourseRequest request, CancellationToken cancellationToken = default)
    {
        ValidateCourseRequest(request);

        var normalizedCourseName = request.CourseName.Trim();
        if (await _dbContext.Courses.AnyAsync(item => item.CourseId != courseId && item.CourseName == normalizedCourseName, cancellationToken))
        {
            throw new InvalidOperationException("Course name already exists.");
        }

        var course = await _dbContext.Courses.FirstOrDefaultAsync(item => item.CourseId == courseId, cancellationToken);
        if (course is null)
        {
            return null;
        }

        course.CourseName = normalizedCourseName;
        course.Description = NormalizeOptional(request.Description);
        course.Price = request.Price;
        course.TotalSlots = request.TotalSlots;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapCourse(course);
    }

    public async Task<bool> DeleteCourseAsync(int courseId, CancellationToken cancellationToken = default)
    {
        var course = await _dbContext.Courses
            .Include(item => item.Classes)
            .FirstOrDefaultAsync(item => item.CourseId == courseId, cancellationToken);

        if (course is null)
        {
            return false;
        }

        if (course.Classes.Count > 0)
        {
            throw new InvalidOperationException("Cannot delete a course that already has classes.");
        }

        _dbContext.Courses.Remove(course);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyCollection<ClassListDto>> GetClassesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Classes.AsNoTracking()
            .Include(item => item.Course)
            .Include(item => item.Teacher)
            .Include(item => item.ClassStudents)
            .OrderBy(item => item.ClassName)
            .Select(item => MapClassList(item))
            .ToListAsync(cancellationToken);
    }

    public async Task<ClassDetailDto?> GetClassByIdAsync(int classId, CancellationToken cancellationToken = default)
    {
        var classEntity = await GetClassAggregateAsync(classId, cancellationToken);
        return classEntity is null ? null : MapClassDetail(classEntity);
    }

    public async Task<ClassDetailDto> CreateClassAsync(CreateClassRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateClassRequestAsync(
            request.ClassName,
            request.CourseId,
            request.TeacherId,
            request.StartDate,
            request.EndDate,
            request.Status,
            cancellationToken);

        var classEntity = new Class
        {
            ClassName = request.ClassName.Trim(),
            CourseId = request.CourseId,
            TeacherId = request.TeacherId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = NormalizeOptional(request.Status) ?? "Opening",
            AllowTeacherGradeComponentManagement = request.AllowTeacherGradeComponentManagement
        };

        _dbContext.Classes.Add(classEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var savedClass = await GetClassAggregateAsync(classEntity.ClassId, cancellationToken)
            ?? throw new InvalidOperationException("Class could not be loaded after creation.");

        return MapClassDetail(savedClass);
    }

    public async Task<ClassDetailDto?> UpdateClassAsync(int classId, UpdateClassRequest request, CancellationToken cancellationToken = default)
    {
        var classEntity = await _dbContext.Classes.FirstOrDefaultAsync(item => item.ClassId == classId, cancellationToken);
        if (classEntity is null)
        {
            return null;
        }

        await ValidateClassRequestAsync(
            request.ClassName,
            request.CourseId,
            classEntity.TeacherId,
            request.StartDate,
            request.EndDate,
            request.Status,
            cancellationToken);

        classEntity.ClassName = request.ClassName.Trim();
        classEntity.CourseId = request.CourseId;
        classEntity.StartDate = request.StartDate;
        classEntity.EndDate = request.EndDate;
        classEntity.Status = NormalizeOptional(request.Status) ?? classEntity.Status;
        classEntity.AllowTeacherGradeComponentManagement = request.AllowTeacherGradeComponentManagement ?? (classEntity.AllowTeacherGradeComponentManagement ?? false);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var savedClass = await GetClassAggregateAsync(classEntity.ClassId, cancellationToken);
        return savedClass is null ? null : MapClassDetail(savedClass);
    }

    public async Task<ClassDetailDto?> UpdateClassGradeComponentPermissionAsync(int classId, bool allowTeacherGradeComponentManagement, CancellationToken cancellationToken = default)
    {
        var classEntity = await _dbContext.Classes.FirstOrDefaultAsync(item => item.ClassId == classId, cancellationToken);
        if (classEntity is null)
        {
            return null;
        }

        classEntity.AllowTeacherGradeComponentManagement = allowTeacherGradeComponentManagement;
        await _dbContext.SaveChangesAsync(cancellationToken);

        var savedClass = await GetClassAggregateAsync(classId, cancellationToken);
        return savedClass is null ? null : MapClassDetail(savedClass);
    }

    public async Task<bool> DeleteClassAsync(int classId, CancellationToken cancellationToken = default)
    {
        var hasAttendance = await _dbContext.Attendances.AsNoTracking()
            .AnyAsync(item => item.Schedule != null && item.Schedule.ClassId == classId, cancellationToken);

        var classEntity = await _dbContext.Classes
            .Include(item => item.ClassStudents)
            .Include(item => item.Schedules)
            .Include(item => item.GradeComponents)
            .FirstOrDefaultAsync(item => item.ClassId == classId, cancellationToken);

        if (classEntity is null)
        {
            return false;
        }

        if (classEntity.ClassStudents.Count > 0 || classEntity.Schedules.Count > 0 || hasAttendance || classEntity.GradeComponents.Count > 0)
        {
            throw new InvalidOperationException("Cannot delete a class that already has related schedules, students, attendance or grades.");
        }

        _dbContext.Classes.Remove(classEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ClassDetailDto?> AssignTeacherAsync(int classId, int teacherId, CancellationToken cancellationToken = default)
    {
        var classEntity = await _dbContext.Classes.FirstOrDefaultAsync(item => item.ClassId == classId, cancellationToken);
        if (classEntity is null)
        {
            return null;
        }

        var teacher = await _dbContext.Users.FirstOrDefaultAsync(item => item.UserId == teacherId, cancellationToken);
        ValidateTeacher(teacher, teacherId);

        classEntity.TeacherId = teacherId;
        await _dbContext.SaveChangesAsync(cancellationToken);

        var savedClass = await GetClassAggregateAsync(classId, cancellationToken);
        return savedClass is null ? null : MapClassDetail(savedClass);
    }

    public async Task<IReadOnlyCollection<ClassStudentDto>?> AssignStudentsAsync(int classId, IReadOnlyCollection<int> studentIds, CancellationToken cancellationToken = default)
    {
        var classExists = await _dbContext.Classes.AnyAsync(item => item.ClassId == classId, cancellationToken);
        if (!classExists)
        {
            return null;
        }

        if (studentIds.Count == 0)
        {
            throw new ArgumentException("At least one student must be provided.", nameof(studentIds));
        }

        var distinctStudentIds = studentIds.Distinct().ToArray();
        var students = await _dbContext.Users
            .Where(item => distinctStudentIds.Contains(item.UserId))
            .ToListAsync(cancellationToken);

        if (students.Count != distinctStudentIds.Length)
        {
            throw new InvalidOperationException("One or more students do not exist.");
        }

        if (students.Any(student => !string.Equals(student.Role, "Student", StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Only users with Student role can be added to a class.");
        }

        var existingStudentIds = await _dbContext.ClassStudents
            .Where(item => item.ClassId == classId && distinctStudentIds.Contains(item.StudentId))
            .Select(item => item.StudentId)
            .ToListAsync(cancellationToken);

        var newAssignments = distinctStudentIds
            .Except(existingStudentIds)
            .Select(studentId => new ClassStudent
            {
                ClassId = classId,
                StudentId = studentId,
                EnrollmentDate = DateTime.UtcNow
            })
            .ToList();

        if (newAssignments.Count > 0)
        {
            _dbContext.ClassStudents.AddRange(newAssignments);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return await _dbContext.ClassStudents.AsNoTracking()
            .Include(item => item.Student)
            .Where(item => item.ClassId == classId)
            .OrderBy(item => item.Student.Fullname)
            .Select(item => MapClassStudent(item))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> RemoveStudentAsync(int classId, int studentId, CancellationToken cancellationToken = default)
    {
        var classStudent = await _dbContext.ClassStudents
            .FirstOrDefaultAsync(item => item.ClassId == classId && item.StudentId == studentId, cancellationToken);

        if (classStudent is null)
        {
            return false;
        }

        _dbContext.ClassStudents.Remove(classStudent);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ClassSchedulePlannerDto?> GetClassSchedulePlannerAsync(int classId, int? month, int? year, int? weekIndex, CancellationToken cancellationToken = default)
    {
        var classEntity = await GetClassAggregateAsync(classId, cancellationToken);
        if (classEntity is null)
        {
            return null;
        }

        return BuildSchedulePlannerDto(classEntity, month, year, weekIndex);
    }

    public async Task<ClassSchedulePlannerDto?> SaveClassSchedulePlannerAsync(int classId, SaveClassSchedulePlannerRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Month is < 1 or > 12)
        {
            throw new ArgumentException("Month must be between 1 and 12.");
        }

        var classEntity = await _dbContext.Classes
            .Include(item => item.Course)
            .Include(item => item.Schedules)
            .FirstOrDefaultAsync(item => item.ClassId == classId, cancellationToken);

        if (classEntity is null)
        {
            return null;
        }

        if (classEntity.Course is null)
        {
            throw new InvalidOperationException("Course for this class was not found.");
        }

        var monthStart = new DateOnly(request.Year, request.Month, 1);
        var weekStart = GetWeekStartForMonth(monthStart, request.WeekIndex);
        var weekEnd = weekStart.AddDays(6);
        var selectedSlots = (request.SelectedSlots ?? []).Distinct().ToHashSet(StringComparer.OrdinalIgnoreCase);
        var allowedSelections = Math.Max(0, classEntity.Course.TotalSlots ?? 0);
        var outsideWeekSchedules = classEntity.Schedules
            .Where(item => !item.ScheduleDate.HasValue || item.ScheduleDate.Value < weekStart || item.ScheduleDate.Value > weekEnd)
            .ToList();

        var weekSchedules = new List<Schedule>();
        foreach (var slotKey in selectedSlots)
        {
            var (scheduleDate, slot) = ParsePlannerSlotSelection(slotKey);
            if (scheduleDate < weekStart || scheduleDate > weekEnd)
            {
                throw new ArgumentException("A selected slot was outside the active week.");
            }

            var isInsideClassRange = (!classEntity.StartDate.HasValue || scheduleDate >= classEntity.StartDate.Value)
                && (!classEntity.EndDate.HasValue || scheduleDate <= classEntity.EndDate.Value);
            if (!isInsideClassRange)
            {
                throw new ArgumentException("A selected slot was outside the class date range.");
            }

            weekSchedules.Add(new Schedule
            {
                ClassId = classId,
                ScheduleDate = scheduleDate,
                DayOfWeek = MapToStoredDayOfWeek(scheduleDate.DayOfWeek),
                StartTime = slot.StartTime,
                EndTime = slot.EndTime,
                Room = request.RoomBySlotKey.TryGetValue(slotKey, out var room) ? NormalizeOptional(room) : null
            });
        }

        var totalSelections = outsideWeekSchedules.Count + weekSchedules.Count;
        if (allowedSelections > 0 && totalSelections > allowedSelections)
        {
            throw new ArgumentException($"This class can only have {allowedSelections} scheduled session(s) based on the course slot rule.");
        }

        var scheduleIds = classEntity.Schedules.Select(s => s.ScheduleId).ToList();
        if (scheduleIds.Count > 0)
        {
            var attendancesToDelete = await _dbContext.Attendances
                .Where(a => scheduleIds.Contains(a.ScheduleId))
                .ToListAsync(cancellationToken);
            _dbContext.Attendances.RemoveRange(attendancesToDelete);

            var teacherCheckInsToDelete = await _dbContext.TeacherCheckIns
                .Where(t => scheduleIds.Contains(t.ScheduleId))
                .ToListAsync(cancellationToken);
            _dbContext.TeacherCheckIns.RemoveRange(teacherCheckInsToDelete);
        }

        _dbContext.Schedules.RemoveRange(classEntity.Schedules);
        if (outsideWeekSchedules.Count > 0)
        {
            _dbContext.Schedules.AddRange(outsideWeekSchedules.Select(item => new Schedule
            {
                ClassId = classId,
                ScheduleDate = item.ScheduleDate,
                DayOfWeek = item.DayOfWeek,
                StartTime = item.StartTime,
                EndTime = item.EndTime,
                Room = NormalizeOptional(item.Room)
            }));
        }

        if (weekSchedules.Count > 0)
        {
            _dbContext.Schedules.AddRange(weekSchedules);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var savedClass = await GetClassAggregateAsync(classId, cancellationToken)
            ?? throw new InvalidOperationException("Class could not be loaded after saving the planner.");

        return BuildSchedulePlannerDto(savedClass, request.Month, request.Year, request.WeekIndex);
    }

    public async Task<IReadOnlyCollection<ScheduleDto>?> UpdateClassSchedulesAsync(int classId, IReadOnlyCollection<UpsertScheduleRequest> schedules, CancellationToken cancellationToken = default)
    {
        var classEntity = await _dbContext.Classes
            .Include(item => item.Course)
            .Include(item => item.Schedules)
            .FirstOrDefaultAsync(item => item.ClassId == classId, cancellationToken);

        if (classEntity is null)
        {
            return null;
        }

        foreach (var schedule in schedules)
        {
            ValidateSchedule(schedule, classEntity.StartDate, classEntity.EndDate);
        }

        var duplicateSlots = schedules
            .GroupBy(item => new { item.ScheduleDate, item.StartTime, item.EndTime })
            .Where(group => group.Count() > 1)
            .ToList();
        if (duplicateSlots.Count > 0)
        {
            throw new ArgumentException("Duplicate schedule slots are not allowed.");
        }

        if (classEntity.Course?.TotalSlots is > 0 && schedules.Count > classEntity.Course.TotalSlots.Value)
        {
            throw new ArgumentException($"Schedule count cannot exceed total slots ({classEntity.Course.TotalSlots.Value}) defined by the course.");
        }

        _dbContext.Schedules.RemoveRange(classEntity.Schedules);

        var updatedSchedules = schedules.Select(schedule => new Schedule
        {
            ClassId = classId,
            ScheduleDate = schedule.ScheduleDate,
            DayOfWeek = schedule.DayOfWeek,
            StartTime = schedule.StartTime,
            EndTime = schedule.EndTime,
            Room = NormalizeOptional(schedule.Room)
        }).ToList();

        if (updatedSchedules.Count > 0)
        {
            _dbContext.Schedules.AddRange(updatedSchedules);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await _dbContext.Schedules.AsNoTracking()
            .Where(item => item.ClassId == classId)
            .OrderBy(item => item.ScheduleDate)
            .ThenBy(item => item.DayOfWeek)
            .ThenBy(item => item.StartTime)
            .Select(item => MapSchedule(item))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ApplicationDto>> GetApplicationsAsync(string? status, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Applications.AsNoTracking()
            .Include(item => item.Sender)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(item => item.Status == status);
        }

        return await query
            .OrderByDescending(item => item.CreatedAt)
            .Select(item => MapApplication(item))
            .ToListAsync(cancellationToken);
    }

    public async Task<ApplicationDto?> RespondApplicationAsync(int applicationId, RespondApplicationRequest request, CancellationToken cancellationToken = default)
    {
        ValidateApplicationStatus(request.Status);

        if (request.AdminResponse?.Length > 1000)
        {
            throw new ArgumentException("Admin response cannot exceed 1000 characters.", nameof(request));
        }

        var application = await _dbContext.Applications
            .Include(item => item.Sender)
            .FirstOrDefaultAsync(item => item.AppId == applicationId, cancellationToken);

        if (application is null)
        {
            return null;
        }

        application.Status = NormalizeStatus(request.Status);
        application.AdminResponse = NormalizeOptional(request.AdminResponse);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapApplication(application);
    }

    private async Task<Class?> GetClassAggregateAsync(int classId, CancellationToken cancellationToken)
    {
        return await _dbContext.Classes.AsNoTracking()
            .Include(item => item.Course)
            .Include(item => item.Teacher)
            .Include(item => item.ClassStudents)
                .ThenInclude(item => item.Student)
            .Include(item => item.Schedules)
            .FirstOrDefaultAsync(item => item.ClassId == classId, cancellationToken);
    }

    private static UserSummaryDto MapUserSummary(User user)
    {
        return new UserSummaryDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Fullname = user.Fullname,
            Email = user.Email,
            Role = user.Role,
            CanManageGradeComponents = user.CanManageGradeComponents ?? false,
            IsActive = user.IsActive ?? true,
            CreatedAt = user.CreatedAt
        };
    }

    private static UserDetailDto MapUserDetail(User user)
    {
        return new UserDetailDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Fullname = user.Fullname,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive ?? true,
            CreatedAt = user.CreatedAt,
            Dob = user.Dob,
            Gender = user.Gender
        };
    }

    private static CourseDto MapCourse(Course course)
    {
        return new CourseDto
        {
            CourseId = course.CourseId,
            CourseName = course.CourseName,
            Description = course.Description,
            Price = course.Price,
            TotalSlots = course.TotalSlots
        };
    }

    private static ClassListDto MapClassList(Class classEntity)
    {
        return new ClassListDto
        {
            ClassId = classEntity.ClassId,
            ClassName = classEntity.ClassName,
            CourseId = classEntity.CourseId,
            CourseName = classEntity.Course.CourseName,
            TeacherId = classEntity.TeacherId,
            TeacherName = classEntity.Teacher?.Fullname,
            StartDate = classEntity.StartDate,
            EndDate = classEntity.EndDate,
            Status = classEntity.Status,
            AllowTeacherGradeComponentManagement = classEntity.AllowTeacherGradeComponentManagement ?? false,
            StudentCount = classEntity.ClassStudents.Count
        };
    }

    private static ClassDetailDto MapClassDetail(Class classEntity)
    {
        return new ClassDetailDto
        {
            ClassId = classEntity.ClassId,
            ClassName = classEntity.ClassName,
            CourseId = classEntity.CourseId,
            CourseName = classEntity.Course.CourseName,
            TeacherId = classEntity.TeacherId,
            TeacherName = classEntity.Teacher?.Fullname,
            StartDate = classEntity.StartDate,
            EndDate = classEntity.EndDate,
            Status = classEntity.Status,
            AllowTeacherGradeComponentManagement = classEntity.AllowTeacherGradeComponentManagement ?? false,
            StudentCount = classEntity.ClassStudents.Count,
            Students = classEntity.ClassStudents
                .OrderBy(item => item.Student.Fullname)
                .Select(item => MapClassStudent(item))
                .ToList(),
            Schedules = classEntity.Schedules
                .OrderBy(item => item.DayOfWeek)
                .ThenBy(item => item.StartTime)
                .Select(item => MapSchedule(item))
                .ToList()
        };
    }

    private static ClassStudentDto MapClassStudent(ClassStudent classStudent)
    {
        return new ClassStudentDto
        {
            StudentId = classStudent.StudentId,
            Fullname = classStudent.Student.Fullname,
            Username = classStudent.Student.Username,
            Email = classStudent.Student.Email,
            EnrollmentDate = classStudent.EnrollmentDate
        };
    }

    private static ScheduleDto MapSchedule(Schedule schedule)
    {
        return new ScheduleDto
        {
            ScheduleId = schedule.ScheduleId,
            ScheduleDate = schedule.ScheduleDate,
            DayOfWeek = schedule.DayOfWeek,
            StartTime = schedule.StartTime,
            EndTime = schedule.EndTime,
            Room = schedule.Room
        };
    }

    private static ApplicationDto MapApplication(Application application)
    {
        return new ApplicationDto
        {
            AppId = application.AppId,
            SenderId = application.SenderId,
            SenderName = application.Sender.Fullname,
            SenderRole = application.Sender.Role ?? string.Empty,
            Title = application.Title,
            Content = application.Content,
            Type = application.Type,
            Status = application.Status,
            AdminResponse = application.AdminResponse,
            CreatedAt = application.CreatedAt
        };
    }

    private static ClassSchedulePlannerDto BuildSchedulePlannerDto(Class classEntity, int? month, int? year, int? weekIndex)
    {
        if (classEntity.Course is null)
        {
            throw new InvalidOperationException("Course for this class was not found.");
        }

        var classDetail = MapClassDetail(classEntity);
        var course = MapCourse(classEntity.Course);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var firstScheduledDate = classEntity.Schedules
            .Where(item => item.ScheduleDate.HasValue)
            .OrderBy(item => item.ScheduleDate)
            .Select(item => item.ScheduleDate)
            .FirstOrDefault();
        var anchorDate = classEntity.StartDate ?? firstScheduledDate ?? today;
        var viewYear = year ?? anchorDate.Year;
        var viewMonth = month ?? anchorDate.Month;
        var monthStart = new DateOnly(viewYear, viewMonth, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
        var resolvedWeekIndex = ResolveWeekIndex(classDetail, monthStart, monthEnd, anchorDate, weekIndex);
        var allowedSelections = Math.Max(0, course.TotalSlots ?? 0);
        var currentSelections = classDetail.Schedules.Count;

        var scheduleLookup = classDetail.Schedules
            .Where(item => item.ScheduleDate.HasValue)
            .Select(item => new { Schedule = item, SlotNumber = ResolveSlotNumber(item.StartTime, item.EndTime) })
            .Where(item => item.SlotNumber > 0)
            .ToDictionary(
                item => BuildSlotKey(item.Schedule.ScheduleDate!.Value, item.SlotNumber),
                item => item.Schedule,
                StringComparer.OrdinalIgnoreCase);

        var weekOptions = new List<SchedulePlannerWeekOptionDto>();
        var firstWeekStart = GetWeekStart(monthStart);
        var lastWeekStart = GetWeekStart(monthEnd);
        var currentWeekIndex = 0;
        for (var weekStart = firstWeekStart; weekStart <= lastWeekStart; weekStart = weekStart.AddDays(7))
        {
            var weekEnd = weekStart.AddDays(6);
            weekOptions.Add(new SchedulePlannerWeekOptionDto
            {
                WeekIndex = currentWeekIndex,
                Label = $"Week {currentWeekIndex + 1}: {weekStart:dd/MM} - {weekEnd:dd/MM}",
                StartDate = weekStart,
                EndDate = weekEnd
            });
            currentWeekIndex++;
        }

        var safeWeekIndex = weekOptions.Count == 0 ? 0 : Math.Clamp(resolvedWeekIndex, 0, weekOptions.Count - 1);
        var activeWeekStart = weekOptions.Count == 0 ? monthStart : weekOptions[safeWeekIndex].StartDate;
        var activeWeekEnd = activeWeekStart.AddDays(6);
        var activeDays = new List<SchedulePlannerDayDto>();
        for (var date = activeWeekStart; date <= activeWeekEnd; date = date.AddDays(1))
        {
            var isInsideClassRange = (!classDetail.StartDate.HasValue || date >= classDetail.StartDate.Value)
                && (!classDetail.EndDate.HasValue || date <= classDetail.EndDate.Value);

            activeDays.Add(new SchedulePlannerDayDto
            {
                Date = date,
                IsCurrentMonth = date.Month == monthStart.Month,
                IsInsideClassRange = isInsideClassRange,
                Slots = FixedSlots.Select(slot =>
                {
                    var key = BuildSlotKey(date, slot.SlotNumber);
                    scheduleLookup.TryGetValue(key, out var existing);

                    return new SchedulePlannerSlotDto
                    {
                        Key = key,
                        SlotNumber = slot.SlotNumber,
                        Label = slot.Label,
                        StartTime = slot.StartTime,
                        EndTime = slot.EndTime,
                        IsSelected = existing is not null,
                        Room = existing?.Room
                    };
                }).ToList()
            });
        }

        var previousMonth = monthStart.AddMonths(-1);
        var nextMonth = monthStart.AddMonths(1);

        return new ClassSchedulePlannerDto
        {
            Class = classDetail,
            Course = course,
            MonthStart = monthStart,
            MonthEnd = monthEnd,
            SelectedMonth = monthStart.Month,
            SelectedYear = monthStart.Year,
            SelectedWeekIndex = safeWeekIndex,
            PreviousMonth = previousMonth.Month,
            PreviousYear = previousMonth.Year,
            NextMonth = nextMonth.Month,
            NextYear = nextMonth.Year,
            SlotDefinitions = FixedSlots,
            WeekOptions = weekOptions,
            CurrentWeek = new SchedulePlannerWeekDto
            {
                WeekIndex = safeWeekIndex,
                StartDate = activeWeekStart,
                EndDate = activeWeekEnd,
                Days = activeDays
            },
            AllowedSelections = allowedSelections,
            CurrentSelections = currentSelections
        };
    }

    private async Task ValidateClassRequestAsync(
        string className,
        int courseId,
        int? teacherId,
        DateOnly? startDate,
        DateOnly? endDate,
        string? status,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(className))
        {
            throw new ArgumentException("ClassName is required.");
        }

        if (className.Trim().Length > 100)
        {
            throw new ArgumentException("ClassName cannot exceed 100 characters.");
        }

        if (courseId <= 0)
        {
            throw new ArgumentException("CourseId is required.");
        }

        if (!await _dbContext.Courses.AnyAsync(item => item.CourseId == courseId, cancellationToken))
        {
            throw new InvalidOperationException("Course does not exist.");
        }

        if (teacherId.HasValue)
        {
            var teacher = await _dbContext.Users.FirstOrDefaultAsync(item => item.UserId == teacherId.Value, cancellationToken);
            ValidateTeacher(teacher, teacherId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status) && !ClassStatuses.Contains(status.Trim()))
        {
            throw new ArgumentException("Status must be one of: Opening, Ongoing, Completed.");
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        if (startDate.HasValue && startDate.Value > today)
        {
            throw new ArgumentException("StartDate cannot be in the future.");
        }

        if (endDate.HasValue && endDate.Value < today)
        {
            throw new ArgumentException("EndDate cannot be in the past.");
        }

        if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
        {
            throw new ArgumentException("StartDate cannot be after EndDate.");
        }
    }

    private static void ValidateTeacher(User? teacher, int teacherId)
    {
        if (teacher is null)
        {
            throw new InvalidOperationException($"Teacher {teacherId} does not exist.");
        }

        if (!string.Equals(teacher.Role, "Teacher", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Assigned user must have Teacher role.");
        }
    }

    private static bool ResolveTeacherGradeComponentPermission(string role, bool requestedPermission)
    {
        return string.Equals(role.Trim(), "Teacher", StringComparison.OrdinalIgnoreCase) && requestedPermission;
    }

    private static void ValidateManagedRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role) || !ManageableRoles.Contains(role.Trim()))
        {
            throw new ArgumentException("Role must be one of: Teacher, Student.");
        }
    }

    private static void ValidateCourseRequest(UpsertCourseRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CourseName))
        {
            throw new ArgumentException("CourseName is required.", nameof(request));
        }

        if (request.CourseName.Trim().Length > 200)
        {
            throw new ArgumentException("CourseName cannot exceed 200 characters.", nameof(request));
        }

        if (request.Description?.Length > 1000)
        {
            throw new ArgumentException("Description cannot exceed 1000 characters.", nameof(request));
        }

        if (request.Price.HasValue && request.Price.Value <= 0)
        {
            throw new ArgumentException("Price must be greater than 0.", nameof(request));
        }

        if (request.TotalSlots.HasValue && request.TotalSlots.Value <= 0)
        {
            throw new ArgumentException("TotalSlots must be greater than 0.", nameof(request));
        }
    }

    private static void ValidateSchedule(UpsertScheduleRequest schedule, DateOnly? classStartDate, DateOnly? classEndDate)
    {
        if (!schedule.ScheduleDate.HasValue)
        {
            throw new ArgumentException("ScheduleDate is required.");
        }

        if (schedule.DayOfWeek is < 2 or > 8)
        {
            throw new ArgumentException("DayOfWeek must be between 2 and 8.");
        }

        if (MapToStoredDayOfWeek(schedule.ScheduleDate.Value.DayOfWeek) != schedule.DayOfWeek)
        {
            throw new ArgumentException("DayOfWeek must match ScheduleDate.");
        }

        if (schedule.StartTime >= schedule.EndTime)
        {
            throw new ArgumentException("StartTime must be earlier than EndTime.");
        }

        if (schedule.Room?.Length > 50)
        {
            throw new ArgumentException("Room cannot exceed 50 characters.");
        }

        if (classStartDate.HasValue && schedule.ScheduleDate.Value < classStartDate.Value)
        {
            throw new ArgumentException("ScheduleDate cannot be earlier than class StartDate.");
        }

        if (classEndDate.HasValue && schedule.ScheduleDate.Value > classEndDate.Value)
        {
            throw new ArgumentException("ScheduleDate cannot be later than class EndDate.");
        }
    }

    private static int ResolveWeekIndex(ClassDetailDto classDetail, DateOnly monthStart, DateOnly monthEnd, DateOnly anchorDate, int? requestedWeekIndex)
    {
        if (requestedWeekIndex.HasValue)
        {
            return Math.Max(0, requestedWeekIndex.Value);
        }

        var firstWeekStart = GetWeekStart(monthStart);
        var lastWeekStart = GetWeekStart(monthEnd);
        var weekCount = ((lastWeekStart.DayNumber - firstWeekStart.DayNumber) / 7) + 1;
        if (weekCount <= 1)
        {
            return 0;
        }

        if (anchorDate >= monthStart && anchorDate <= monthEnd)
        {
            return Math.Clamp((GetWeekStart(anchorDate).DayNumber - firstWeekStart.DayNumber) / 7, 0, weekCount - 1);
        }

        for (var index = 0; index < weekCount; index++)
        {
            var weekStart = firstWeekStart.AddDays(index * 7);
            var hasSelectableDay = Enumerable.Range(0, 7)
                .Select(offset => weekStart.AddDays(offset))
                .Any(date => date >= monthStart
                    && date <= monthEnd
                    && (!classDetail.StartDate.HasValue || date >= classDetail.StartDate.Value)
                    && (!classDetail.EndDate.HasValue || date <= classDetail.EndDate.Value));

            if (hasSelectableDay)
            {
                return index;
            }
        }

        return 0;
    }

    private static DateOnly GetWeekStart(DateOnly date)
    {
        return date.AddDays(-((int)date.DayOfWeek + 6) % 7);
    }

    private static DateOnly GetWeekStartForMonth(DateOnly monthStart, int weekIndex)
    {
        return GetWeekStart(monthStart).AddDays(Math.Max(0, weekIndex) * 7);
    }

    private static int MapToStoredDayOfWeek(DayOfWeek dayOfWeek)
    {
        return dayOfWeek == DayOfWeek.Sunday ? 8 : ((int)dayOfWeek) + 1;
    }

    private static string BuildSlotKey(DateOnly date, int slotNumber)
    {
        return $"{date:yyyy-MM-dd}|{slotNumber}";
    }

    private static int ResolveSlotNumber(TimeOnly startTime, TimeOnly endTime)
    {
        var slot = FixedSlots.FirstOrDefault(item => item.StartTime == startTime && item.EndTime == endTime);
        return slot?.SlotNumber ?? 0;
    }

    private static (DateOnly ScheduleDate, ScheduleSlotDefinitionDto Slot) ParsePlannerSlotSelection(string slotKey)
    {
        var parts = slotKey.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            throw new ArgumentException("An invalid schedule selection was detected.");
        }

        if (!DateOnly.TryParse(parts[0], CultureInfo.InvariantCulture, DateTimeStyles.None, out var scheduleDate))
        {
            throw new ArgumentException("An invalid schedule date was detected.");
        }

        if (!int.TryParse(parts[1], out var slotNumber))
        {
            throw new ArgumentException("An invalid slot number was detected.");
        }

        var slot = FixedSlots.FirstOrDefault(item => item.SlotNumber == slotNumber)
            ?? throw new ArgumentException("An invalid slot number was detected.");

        return (scheduleDate, slot);
    }

    private static void ValidateApplicationStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status) || !ApplicationStatuses.Contains(status.Trim()))
        {
            throw new ArgumentException("Status must be one of: Pending, Approved, Rejected.");
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string NormalizeRole(string role)
    {
        return role.Trim() switch
        {
            var value when value.Equals("Teacher", StringComparison.OrdinalIgnoreCase) => "Teacher",
            var value when value.Equals("Student", StringComparison.OrdinalIgnoreCase) => "Student",
            _ => role.Trim()
        };
    }

    private static string NormalizeStatus(string status)
    {
        return status.Trim() switch
        {
            var value when value.Equals("Pending", StringComparison.OrdinalIgnoreCase) => "Pending",
            var value when value.Equals("Approved", StringComparison.OrdinalIgnoreCase) => "Approved",
            var value when value.Equals("Rejected", StringComparison.OrdinalIgnoreCase) => "Rejected",
            _ => status.Trim()
        };
    }

    private static void ValidateCreateUserRequest(CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            throw new ArgumentException("Username is required.", nameof(request));
        }

        if (request.Username.Trim().Length > 50)
        {
            throw new ArgumentException("Username cannot exceed 50 characters.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Fullname))
        {
            throw new ArgumentException("Fullname is required.", nameof(request));
        }

        if (request.Fullname.Trim().Length > 100)
        {
            throw new ArgumentException("Fullname cannot exceed 100 characters.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Password is required.", nameof(request));
        }

        var passwordError = ValidatePasswordComplexity(request.Password);
        if (passwordError is not null)
        {
            throw new ArgumentException(passwordError, nameof(request));
        }

        ValidateOptionalEmail(request.Email);
        ValidateOptionalGender(request.Gender);
        ValidateOptionalDob(request.Dob);
    }

    private static void ValidateUpdateUserRequest(UpdateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Fullname))
        {
            throw new ArgumentException("Fullname is required.", nameof(request));
        }

        if (request.Fullname.Trim().Length > 100)
        {
            throw new ArgumentException("Fullname cannot exceed 100 characters.", nameof(request));
        }

        ValidateOptionalEmail(request.Email);
        ValidateOptionalGender(request.Gender);
        ValidateOptionalDob(request.Dob);
    }

    private static void ValidateOptionalEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return;
        }

        if (email.Trim().Length > 100)
        {
            throw new ArgumentException("Email cannot exceed 100 characters.");
        }

        try
        {
            _ = new System.Net.Mail.MailAddress(email.Trim());
        }
        catch (FormatException)
        {
            throw new ArgumentException("Email is not valid.");
        }
    }

    private static void ValidateOptionalGender(string? gender)
    {
        if (string.IsNullOrWhiteSpace(gender))
        {
            return;
        }

        if (gender.Trim().Length > 10)
        {
            throw new ArgumentException("Gender cannot exceed 10 characters.");
        }

        if (!AllowedGenders.Contains(gender.Trim()))
        {
            throw new ArgumentException("Gender must be one of: Male, Female, Other.");
        }
    }

    private static void ValidateOptionalDob(DateOnly? dob)
    {
        if (!dob.HasValue)
        {
            return;
        }

        if (dob.Value > DateOnly.FromDateTime(DateTime.Today))
        {
            throw new ArgumentException("Date of birth cannot be in the future.");
        }
    }

    private static string? ValidatePasswordComplexity(string password)
    {
        if (password.Length < 8)
        {
            return "Password must be at least 8 characters.";
        }

        if (!password.Any(char.IsUpper))
        {
            return "Password must contain at least one uppercase letter.";
        }

        if (!password.Any(char.IsLower))
        {
            return "Password must contain at least one lowercase letter.";
        }

        if (!password.Any(char.IsDigit))
        {
            return "Password must contain at least one digit.";
        }

        if (!Regex.IsMatch(password, "[^a-zA-Z0-9]"))
        {
            return "Password must contain at least one special character.";
        }

        return null;
    }

    private static string GeneratePassword()
    {
        const string allowedChars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$";
        var bytes = RandomNumberGenerator.GetBytes(12);
        var passwordChars = new char[bytes.Length];

        for (var index = 0; index < bytes.Length; index++)
        {
            passwordChars[index] = allowedChars[bytes[index] % allowedChars.Length];
        }

        return new string(passwordChars);
    }
}