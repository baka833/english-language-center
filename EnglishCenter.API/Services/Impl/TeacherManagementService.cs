using EnglishCenter.API.DTOs;
using EnglishCenter.API.Models;
using EnglishCenter.API.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.API.Services.Impl;

public sealed class TeacherManagementService : ITeacherManagementService
{
    private static readonly HashSet<string> AttendanceStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Present",
        "Absent",
        "Late",
        "Excused"
    };

    private readonly EnglishCenterDbContext _dbContext;

    public TeacherManagementService(EnglishCenterDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<TeacherAssignedClassDto>> GetAssignedClassesAsync(int teacherId, CancellationToken cancellationToken = default)
    {
        var teacher = await EnsureTeacherAsync(teacherId, cancellationToken);

        return await _dbContext.Classes.AsNoTracking()
            .Include(item => item.Course)
            .Include(item => item.ClassStudents)
            .Include(item => item.Schedules)
            .Where(item => item.TeacherId == teacherId
                && (item.Status == "Opening" || item.Status == "Ongoing"))
            .OrderBy(item => item.ClassName)
            .Select(item => MapAssignedClass(item, teacher))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<TeacherScheduleItemDto>> GetTeacherScheduleAsync(int teacherId, DateOnly? fromDate, DateOnly? toDate, int? month, int? year, CancellationToken cancellationToken = default)
    {
        await EnsureTeacherAsync(teacherId, cancellationToken);

        var dateWindow = ResolveDateWindow(fromDate, toDate, month, year);

        var query = _dbContext.Schedules.AsNoTracking()
            .Include(item => item.Class)
                .ThenInclude(item => item.Course)
            .Where(item => item.Class.TeacherId == teacherId)
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
            .Select(item => new TeacherScheduleItemDto
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

    public async Task<IReadOnlyCollection<TeacherStudentDto>?> GetStudentsByClassAsync(int teacherId, int classId, CancellationToken cancellationToken = default)
    {
        if (!await IsAssignedClassAsync(teacherId, classId, cancellationToken))
        {
            return null;
        }

        return await _dbContext.ClassStudents.AsNoTracking()
            .Include(item => item.Student)
            .Where(item => item.ClassId == classId)
            .OrderBy(item => item.Student.Fullname)
            .Select(item => MapTeacherStudent(item))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<AttendanceSlotDto>?> GetAttendanceSlotsAsync(int teacherId, int classId, DateOnly attendanceDate, CancellationToken cancellationToken = default)
    {
        if (!await IsAssignedClassAsync(teacherId, classId, cancellationToken))
        {
            return null;
        }

        var storedDayOfWeek = MapToStoredDayOfWeek(attendanceDate.DayOfWeek);
        var schedules = await _dbContext.Schedules.AsNoTracking()
            .Where(item => item.ClassId == classId
                && ((!item.ScheduleDate.HasValue && item.DayOfWeek == storedDayOfWeek)
                    || (item.ScheduleDate.HasValue && item.ScheduleDate.Value == attendanceDate)))
            .OrderBy(item => item.StartTime)
            .ThenBy(item => item.EndTime)
            .ToListAsync(cancellationToken);

        var scheduleIds = schedules.Select(item => item.ScheduleId).ToArray();
        var checkIns = await _dbContext.TeacherCheckIns.AsNoTracking()
            .Where(item => item.TeacherId == teacherId
                && item.AttendanceDate == attendanceDate
                && scheduleIds.Contains(item.ScheduleId))
            .ToListAsync(cancellationToken);
        var checkInLookup = checkIns.ToDictionary(item => item.ScheduleId);

        var attendanceCounts = await _dbContext.Attendances.AsNoTracking()
            .Where(item => item.AttendanceDate == attendanceDate
                && scheduleIds.Contains(item.ScheduleId)
                && !string.IsNullOrWhiteSpace(item.Status))
            .GroupBy(item => item.ScheduleId)
            .Select(item => new { ScheduleId = item.Key, Count = item.Count() })
            .ToListAsync(cancellationToken);
        var attendanceLookup = attendanceCounts.ToDictionary(item => item.ScheduleId, item => item.Count);

        return schedules.Select(item =>
        {
            checkInLookup.TryGetValue(item.ScheduleId, out var checkIn);
            return new AttendanceSlotDto
            {
                ScheduleId = item.ScheduleId,
                ScheduleDate = item.ScheduleDate,
                DayOfWeek = item.DayOfWeek,
                StartTime = item.StartTime,
                EndTime = item.EndTime,
                Room = item.Room,
                IsCheckedIn = checkIn is not null,
                CheckedInAt = checkIn?.CheckedInAt,
                RecordedStudents = attendanceLookup.GetValueOrDefault(item.ScheduleId)
            };
        }).ToList();
    }

    public async Task<AttendanceSlotDto?> CheckInAttendanceSlotAsync(int teacherId, int classId, TeacherAttendanceCheckInRequest request, CancellationToken cancellationToken = default)
    {
        if (!await IsAssignedClassAsync(teacherId, classId, cancellationToken))
        {
            return null;
        }

        if (request.AttendanceDate > DateOnly.FromDateTime(DateTime.Today))
        {
            throw new ArgumentException("Check-in cannot be created for a future date.", nameof(request));
        }

        var schedule = await EnsureValidAttendanceSlotAsync(classId, request.ScheduleId, request.AttendanceDate, cancellationToken);
        var checkIn = await _dbContext.TeacherCheckIns
            .FirstOrDefaultAsync(item => item.TeacherId == teacherId
                && item.ScheduleId == request.ScheduleId
                && item.AttendanceDate == request.AttendanceDate, cancellationToken);

        if (checkIn is null)
        {
            checkIn = new TeacherCheckIn
            {
                TeacherId = teacherId,
                ScheduleId = request.ScheduleId,
                AttendanceDate = request.AttendanceDate,
                CheckedInAt = DateTime.UtcNow
            };

            _dbContext.TeacherCheckIns.Add(checkIn);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var recordedStudents = await _dbContext.Attendances.AsNoTracking()
            .Where(item => item.AttendanceDate == request.AttendanceDate
                && item.ScheduleId == request.ScheduleId
                && !string.IsNullOrWhiteSpace(item.Status))
            .CountAsync(cancellationToken);

        return new AttendanceSlotDto
        {
            ScheduleId = schedule.ScheduleId,
            ScheduleDate = schedule.ScheduleDate,
            DayOfWeek = schedule.DayOfWeek,
            StartTime = schedule.StartTime,
            EndTime = schedule.EndTime,
            Room = schedule.Room,
            IsCheckedIn = true,
            CheckedInAt = checkIn.CheckedInAt,
            RecordedStudents = recordedStudents
        };
    }

    public async Task<IReadOnlyCollection<AttendanceRecordDto>?> GetAttendanceByDateAsync(int teacherId, int classId, DateOnly attendanceDate, int scheduleId, CancellationToken cancellationToken = default)
    {
        return await GetAttendanceRecordsAsync(teacherId, classId, attendanceDate, scheduleId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<AttendanceRecordDto>?> UpsertAttendanceAsync(int teacherId, int classId, UpsertAttendanceRequest request, CancellationToken cancellationToken = default)
    {
        if (!await IsAssignedClassAsync(teacherId, classId, cancellationToken))
        {
            return null;
        }

        if (request.AttendanceDate > DateOnly.FromDateTime(DateTime.Today))
        {
            throw new ArgumentException("Attendance cannot be recorded for a future date.", nameof(request));
        }

        if (request.Records.Count == 0)
        {
            throw new ArgumentException("Attendance records are required.", nameof(request));
        }

        await EnsureValidAttendanceSlotAsync(classId, request.ScheduleId, request.AttendanceDate, cancellationToken);

        var hasCheckIn = await _dbContext.TeacherCheckIns.AsNoTracking()
            .AnyAsync(item => item.TeacherId == teacherId
                && item.ScheduleId == request.ScheduleId
                && item.AttendanceDate == request.AttendanceDate, cancellationToken);

        if (!hasCheckIn)
        {
            throw new InvalidOperationException("Please check in for this slot before taking attendance.");
        }

        var distinctStudentIds = request.Records.Select(item => item.StudentId).Distinct().ToArray();
        if (distinctStudentIds.Length != request.Records.Count)
        {
            throw new ArgumentException("Duplicate attendance records for the same student are not allowed.", nameof(request));
        }

        foreach (var record in request.Records)
        {
            ValidateAttendanceStatus(record.Status);
        }

        var enrolledStudents = await _dbContext.ClassStudents
            .Where(item => item.ClassId == classId && distinctStudentIds.Contains(item.StudentId))
            .Select(item => item.StudentId)
            .ToListAsync(cancellationToken);

        if (enrolledStudents.Count != distinctStudentIds.Length)
        {
            throw new InvalidOperationException("One or more students do not belong to this class.");
        }

        var existingRecords = await _dbContext.Attendances
            .Where(item => item.AttendanceDate == request.AttendanceDate
                && item.ScheduleId == request.ScheduleId
                && distinctStudentIds.Contains(item.StudentId))
            .ToListAsync(cancellationToken);

        var existingLookup = existingRecords.ToDictionary(item => item.StudentId);

        foreach (var record in request.Records)
        {
            if (existingLookup.TryGetValue(record.StudentId, out var attendance))
            {
                attendance.Status = NormalizeStatus(record.Status);
                attendance.Note = NormalizeOptional(record.Note);
                continue;
            }

            _dbContext.Attendances.Add(new Attendance
            {
                ScheduleId = request.ScheduleId,
                StudentId = record.StudentId,
                AttendanceDate = request.AttendanceDate,
                Status = NormalizeStatus(record.Status),
                Note = NormalizeOptional(record.Note)
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetAttendanceRecordsAsync(teacherId, classId, request.AttendanceDate, request.ScheduleId, cancellationToken);
    }

    public async Task<AttendanceSummaryDto?> GetAttendanceSummaryAsync(int teacherId, int classId, CancellationToken cancellationToken = default)
    {
        if (!await IsAssignedClassAsync(teacherId, classId, cancellationToken))
        {
            return null;
        }

        var classData = await _dbContext.Classes.AsNoTracking()
            .Include(item => item.ClassStudents)
                .ThenInclude(item => item.Student)
            .FirstOrDefaultAsync(item => item.ClassId == classId, cancellationToken);

        if (classData is null)
        {
            return null;
        }

        var classScheduleIds = await _dbContext.Schedules.AsNoTracking()
            .Where(item => item.ClassId == classId)
            .Select(item => item.ScheduleId)
            .ToListAsync(cancellationToken);

        var attendanceRecords = await _dbContext.Attendances.AsNoTracking()
            .Where(item => classScheduleIds.Contains(item.ScheduleId))
            .ToListAsync(cancellationToken);

        var totalSessions = attendanceRecords
            .Select(item => new { item.ScheduleId, item.AttendanceDate })
            .Distinct()
            .Count();

        var studentSummaries = classData.ClassStudents
            .OrderBy(item => item.Student.Fullname)
            .Select(item =>
            {
                var studentRecords = attendanceRecords.Where(record => record.StudentId == item.StudentId).ToList();

                return new StudentAttendanceSummaryDto
                {
                    StudentId = item.StudentId,
                    StudentName = item.Student.Fullname,
                    MarkedSessions = studentRecords.Count,
                    PresentCount = studentRecords.Count(record => string.Equals(record.Status, "Present", StringComparison.OrdinalIgnoreCase)),
                    AbsentCount = studentRecords.Count(record => string.Equals(record.Status, "Absent", StringComparison.OrdinalIgnoreCase)),
                    LateCount = studentRecords.Count(record => string.Equals(record.Status, "Late", StringComparison.OrdinalIgnoreCase)),
                    ExcusedCount = studentRecords.Count(record => string.Equals(record.Status, "Excused", StringComparison.OrdinalIgnoreCase))
                };
            })
            .ToList();

        return new AttendanceSummaryDto
        {
            ClassId = classData.ClassId,
            ClassName = classData.ClassName,
            TotalSessions = totalSessions,
            Students = studentSummaries
        };
    }

    public async Task<IReadOnlyCollection<TeacherGradeComponentDto>?> GetGradeComponentsAsync(int teacherId, int classId, CancellationToken cancellationToken = default)
    {
        if (!await IsAssignedClassAsync(teacherId, classId, cancellationToken))
        {
            return null;
        }

        return await _dbContext.GradeComponents.AsNoTracking()
            .Where(item => item.ClassId == classId)
            .OrderBy(item => item.ComponentName)
            .Select(item => MapGradeComponent(item))
            .ToListAsync(cancellationToken);
    }

    public async Task<TeacherGradeComponentDto?> CreateGradeComponentAsync(int teacherId, int classId, UpsertGradeComponentRequest request, CancellationToken cancellationToken = default)
    {
        if (!await IsAssignedClassAsync(teacherId, classId, cancellationToken))
        {
            return null;
        }

        await EnsureGradeComponentManagementPermissionAsync(teacherId, classId, cancellationToken);

        ValidateGradeComponentRequest(request);

        var currentTotalWeight = await _dbContext.GradeComponents
            .Where(item => item.ClassId == classId)
            .SumAsync(item => item.Weight, cancellationToken);

        if (currentTotalWeight + request.Weight > 100)
        {
            throw new InvalidOperationException("Total grade component weight cannot exceed 100.");
        }

        var component = new GradeComponent
        {
            ClassId = classId,
            ComponentName = request.ComponentName.Trim(),
            Weight = request.Weight
        };

        _dbContext.GradeComponents.Add(component);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapGradeComponent(component);
    }

    public async Task<TeacherGradeComponentDto?> UpdateGradeComponentAsync(int teacherId, int classId, int componentId, UpsertGradeComponentRequest request, CancellationToken cancellationToken = default)
    {
        if (!await IsAssignedClassAsync(teacherId, classId, cancellationToken))
        {
            return null;
        }

        await EnsureGradeComponentManagementPermissionAsync(teacherId, classId, cancellationToken);

        ValidateGradeComponentRequest(request);

        var component = await _dbContext.GradeComponents
            .FirstOrDefaultAsync(item => item.ClassId == classId && item.ComponentId == componentId, cancellationToken);

        if (component is null)
        {
            return null;
        }

        var currentTotalWeight = await _dbContext.GradeComponents
            .Where(item => item.ClassId == classId && item.ComponentId != componentId)
            .SumAsync(item => item.Weight, cancellationToken);

        if (currentTotalWeight + request.Weight > 100)
        {
            throw new InvalidOperationException("Total grade component weight cannot exceed 100.");
        }

        component.ComponentName = request.ComponentName.Trim();
        component.Weight = request.Weight;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapGradeComponent(component);
    }

    public async Task<bool> DeleteGradeComponentAsync(int teacherId, int classId, int componentId, CancellationToken cancellationToken = default)
    {
        if (!await IsAssignedClassAsync(teacherId, classId, cancellationToken))
        {
            return false;
        }

        await EnsureGradeComponentManagementPermissionAsync(teacherId, classId, cancellationToken);

        var component = await _dbContext.GradeComponents
            .Include(item => item.Grades)
            .FirstOrDefaultAsync(item => item.ClassId == classId && item.ComponentId == componentId, cancellationToken);

        if (component is null)
        {
            return false;
        }

        if (component.Grades.Count > 0)
        {
            throw new InvalidOperationException("Cannot delete a grade component that already has grades.");
        }

        _dbContext.GradeComponents.Remove(component);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyCollection<GradeEntryDto>?> GetGradesAsync(int teacherId, int classId, int componentId, CancellationToken cancellationToken = default)
    {
        if (!await IsAssignedClassAsync(teacherId, classId, cancellationToken))
        {
            return null;
        }

        var component = await _dbContext.GradeComponents.AsNoTracking()
            .FirstOrDefaultAsync(item => item.ClassId == classId && item.ComponentId == componentId, cancellationToken);

        if (component is null)
        {
            return null;
        }

        var classStudents = await _dbContext.ClassStudents.AsNoTracking()
            .Include(item => item.Student)
            .Where(item => item.ClassId == classId)
            .OrderBy(item => item.Student.Fullname)
            .ToListAsync(cancellationToken);

        var grades = await _dbContext.Grades.AsNoTracking()
            .Where(item => item.ComponentId == componentId)
            .ToListAsync(cancellationToken);

        var gradeLookup = grades.ToDictionary(item => item.StudentId);

        return classStudents.Select(item =>
        {
            gradeLookup.TryGetValue(item.StudentId, out var grade);

            return new GradeEntryDto
            {
                GradeId = grade?.GradeId,
                ComponentId = component.ComponentId,
                ComponentName = component.ComponentName,
                StudentId = item.StudentId,
                StudentName = item.Student.Fullname,
                GradeValue = grade?.GradeValue,
                TeacherComment = grade?.TeacherComment,
                UpdatedAt = grade?.UpdatedAt
            };
        }).ToList();
    }

    public async Task<GradeEntryDto?> UpsertGradeAsync(int teacherId, int classId, int componentId, UpsertGradeRequest request, CancellationToken cancellationToken = default)
    {
        if (!await IsAssignedClassAsync(teacherId, classId, cancellationToken))
        {
            return null;
        }

        ValidateGradeRequest(request);

        var component = await _dbContext.GradeComponents
            .FirstOrDefaultAsync(item => item.ClassId == classId && item.ComponentId == componentId, cancellationToken);

        if (component is null)
        {
            return null;
        }

        var student = await _dbContext.ClassStudents
            .Include(item => item.Student)
            .FirstOrDefaultAsync(item => item.ClassId == classId && item.StudentId == request.StudentId, cancellationToken);

        if (student is null)
        {
            throw new InvalidOperationException("Student does not belong to this class.");
        }

        var grade = await _dbContext.Grades
            .FirstOrDefaultAsync(item => item.ComponentId == componentId && item.StudentId == request.StudentId, cancellationToken);

        if (grade is null)
        {
            grade = new Grade
            {
                ComponentId = componentId,
                StudentId = request.StudentId,
                GradeValue = request.GradeValue,
                TeacherComment = NormalizeOptional(request.TeacherComment),
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.Grades.Add(grade);
        }
        else
        {
            grade.GradeValue = request.GradeValue;
            grade.TeacherComment = NormalizeOptional(request.TeacherComment);
            grade.UpdatedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new GradeEntryDto
        {
            GradeId = grade.GradeId,
            ComponentId = component.ComponentId,
            ComponentName = component.ComponentName,
            StudentId = student.StudentId,
            StudentName = student.Student.Fullname,
            GradeValue = grade.GradeValue,
            TeacherComment = grade.TeacherComment,
            UpdatedAt = grade.UpdatedAt
        };
    }

    public async Task<IReadOnlyCollection<ApplicationDto>> GetApplicationsAsync(int teacherId, CancellationToken cancellationToken = default)
    {
        var teacher = await EnsureTeacherAsync(teacherId, cancellationToken);

        return await _dbContext.Applications.AsNoTracking()
            .Where(item => item.SenderId == teacherId)
            .OrderByDescending(item => item.CreatedAt)
            .Select(item => new ApplicationDto
            {
                AppId = item.AppId,
                SenderId = item.SenderId,
                SenderName = teacher.Fullname,
                SenderRole = teacher.Role ?? string.Empty,
                Title = item.Title,
                Content = item.Content,
                Type = item.Type,
                Status = item.Status,
                AdminResponse = item.AdminResponse,
                CreatedAt = item.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ApplicationDto> CreateApplicationAsync(int teacherId, CreateTeacherApplicationRequest request, CancellationToken cancellationToken = default)
    {
        var teacher = await EnsureTeacherAsync(teacherId, cancellationToken);

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
            SenderId = teacherId,
            Title = request.Title.Trim(),
            Content = NormalizeOptional(request.Content),
            Type = request.Type.Trim(),
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            Sender = teacher
        };

        _dbContext.Applications.Add(application);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApplicationDto
        {
            AppId = application.AppId,
            SenderId = application.SenderId,
            SenderName = teacher.Fullname,
            SenderRole = teacher.Role ?? string.Empty,
            Title = application.Title,
            Content = application.Content,
            Type = application.Type,
            Status = application.Status,
            AdminResponse = application.AdminResponse,
            CreatedAt = application.CreatedAt
        };
    }

    private async Task<User> EnsureTeacherAsync(int teacherId, CancellationToken cancellationToken)
    {
        var teacher = await _dbContext.Users.FirstOrDefaultAsync(item => item.UserId == teacherId, cancellationToken);
        if (teacher is null)
        {
            throw new ArgumentException("Teacher was not found.", nameof(teacherId));
        }

        if (!string.Equals(teacher.Role, "Teacher", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The specified user is not a teacher.");
        }

        return teacher;
    }

    private async Task<bool> IsAssignedClassAsync(int teacherId, int classId, CancellationToken cancellationToken)
    {
        await EnsureTeacherAsync(teacherId, cancellationToken);

        return await _dbContext.Classes.AsNoTracking()
            .AnyAsync(item => item.ClassId == classId && item.TeacherId == teacherId, cancellationToken);
    }

    private async Task EnsureGradeComponentManagementPermissionAsync(int teacherId, int classId, CancellationToken cancellationToken)
    {
        var teacherAndClass = await _dbContext.Classes.AsNoTracking()
            .Include(item => item.Teacher)
            .Where(item => item.ClassId == classId && item.TeacherId == teacherId)
            .Select(item => new
            {
                TeacherPermission = item.Teacher != null && (item.Teacher.CanManageGradeComponents ?? false),
                ClassPermission = item.AllowTeacherGradeComponentManagement ?? false
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (teacherAndClass is null)
        {
            throw new InvalidOperationException("Teacher is not assigned to this class.");
        }

        if (!teacherAndClass.TeacherPermission && !teacherAndClass.ClassPermission)
        {
            throw new InvalidOperationException("Admin has not granted grade component management permission for this teacher or class.");
        }
    }

    private async Task<IReadOnlyCollection<AttendanceRecordDto>?> GetAttendanceRecordsAsync(int teacherId, int classId, DateOnly attendanceDate, int scheduleId, CancellationToken cancellationToken)
    {
        if (!await IsAssignedClassAsync(teacherId, classId, cancellationToken))
        {
            return null;
        }

        await EnsureValidAttendanceSlotAsync(classId, scheduleId, attendanceDate, cancellationToken);

        var students = await _dbContext.ClassStudents.AsNoTracking()
            .Include(item => item.Student)
            .Where(item => item.ClassId == classId)
            .OrderBy(item => item.Student.Fullname)
            .ToListAsync(cancellationToken);

        var attendances = await _dbContext.Attendances.AsNoTracking()
            .Where(item => item.AttendanceDate == attendanceDate
                && item.ScheduleId == scheduleId)
            .ToListAsync(cancellationToken);

        var attendanceLookup = attendances.ToDictionary(item => item.StudentId);

        return students.Select(item =>
        {
            attendanceLookup.TryGetValue(item.StudentId, out var attendance);

            return new AttendanceRecordDto
            {
                AttendanceId = attendance?.AttendanceId,
                ScheduleId = scheduleId,
                StudentId = item.StudentId,
                StudentName = item.Student.Fullname,
                AttendanceDate = attendanceDate,
                Status = attendance?.Status,
                Note = attendance?.Note
            };
        }).ToList();
    }

    private async Task<Schedule> EnsureValidAttendanceSlotAsync(int classId, int scheduleId, DateOnly attendanceDate, CancellationToken cancellationToken)
    {
        var schedule = await _dbContext.Schedules.AsNoTracking()
            .FirstOrDefaultAsync(item => item.ScheduleId == scheduleId && item.ClassId == classId, cancellationToken);

        if (schedule is null)
        {
            throw new ArgumentException("The selected slot was not found for this class.");
        }

        if (schedule.ScheduleDate.HasValue && schedule.ScheduleDate.Value != attendanceDate)
        {
            throw new InvalidOperationException("The selected slot does not belong to the selected attendance date.");
        }

        if (!schedule.ScheduleDate.HasValue && schedule.DayOfWeek != MapToStoredDayOfWeek(attendanceDate.DayOfWeek))
        {
            throw new InvalidOperationException("The selected slot does not match the selected attendance date.");
        }

        return schedule;
    }

    private static int MapToStoredDayOfWeek(DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Monday => 2,
            DayOfWeek.Tuesday => 3,
            DayOfWeek.Wednesday => 4,
            DayOfWeek.Thursday => 5,
            DayOfWeek.Friday => 6,
            DayOfWeek.Saturday => 7,
            DayOfWeek.Sunday => 8,
            _ => throw new ArgumentOutOfRangeException(nameof(dayOfWeek), dayOfWeek, "Unsupported day of week.")
        };
    }

    private static TeacherAssignedClassDto MapAssignedClass(Class classEntity, User teacher)
    {
        var teacherPermission = teacher.CanManageGradeComponents ?? false;
        var classPermission = classEntity.AllowTeacherGradeComponentManagement ?? false;

        return new TeacherAssignedClassDto
        {
            ClassId = classEntity.ClassId,
            ClassName = classEntity.ClassName,
            CourseId = classEntity.CourseId,
            CourseName = classEntity.Course.CourseName,
            StartDate = classEntity.StartDate,
            EndDate = classEntity.EndDate,
            Status = classEntity.Status,
            AllowTeacherGradeComponentManagement = classPermission,
            TeacherCanManageGradeComponents = teacherPermission,
            EffectiveCanManageGradeComponents = teacherPermission || classPermission,
            StudentCount = classEntity.ClassStudents.Count,
            Schedules = classEntity.Schedules
                .OrderBy(item => item.DayOfWeek)
                .ThenBy(item => item.StartTime)
                .Select(item => new ScheduleDto
                {
                    ScheduleId = item.ScheduleId,
                    ScheduleDate = item.ScheduleDate,
                    DayOfWeek = item.DayOfWeek,
                    StartTime = item.StartTime,
                    EndTime = item.EndTime,
                    Room = item.Room
                })
                .ToList()
        };
    }

    private static TeacherStudentDto MapTeacherStudent(ClassStudent classStudent)
    {
        return new TeacherStudentDto
        {
            StudentId = classStudent.StudentId,
            Fullname = classStudent.Student.Fullname,
            Username = classStudent.Student.Username,
            Email = classStudent.Student.Email,
            Dob = classStudent.Student.Dob,
            EnrollmentDate = classStudent.EnrollmentDate
        };
    }

    private static TeacherGradeComponentDto MapGradeComponent(GradeComponent component)
    {
        return new TeacherGradeComponentDto
        {
            ComponentId = component.ComponentId,
            ClassId = component.ClassId,
            ComponentName = component.ComponentName,
            Weight = component.Weight
        };
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

    private static void ValidateAttendanceStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status) || !AttendanceStatuses.Contains(status.Trim()))
        {
            throw new ArgumentException("Attendance status must be Present, Absent, Late or Excused.");
        }
    }

    private static void ValidateGradeComponentRequest(UpsertGradeComponentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ComponentName))
        {
            throw new ArgumentException("Component name is required.", nameof(request));
        }

        if (request.Weight <= 0 || request.Weight > 100)
        {
            throw new ArgumentException("Component weight must be greater than 0 and less than or equal to 100.", nameof(request));
        }
    }

    private static void ValidateGradeRequest(UpsertGradeRequest request)
    {
        if (request.GradeValue.HasValue && (request.GradeValue.Value < 0 || request.GradeValue.Value > 10))
        {
            throw new ArgumentException("Grade value must be between 0 and 10.", nameof(request));
        }
    }

    private static string NormalizeStatus(string value)
    {
        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}