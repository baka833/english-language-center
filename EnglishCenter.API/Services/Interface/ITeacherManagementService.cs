using EnglishCenter.API.DTOs.Admin;
using EnglishCenter.API.DTOs.Teacher;

namespace EnglishCenter.API.Services.Interface;

public interface ITeacherManagementService
{
    Task<IReadOnlyCollection<TeacherAssignedClassDto>> GetAssignedClassesAsync(int teacherId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TeacherScheduleItemDto>> GetTeacherScheduleAsync(int teacherId, DateOnly? fromDate, DateOnly? toDate, int? month, int? year, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TeacherStudentDto>?> GetStudentsByClassAsync(int teacherId, int classId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<AttendanceSlotDto>?> GetAttendanceSlotsAsync(int teacherId, int classId, DateOnly attendanceDate, CancellationToken cancellationToken = default);

    Task<AttendanceSlotDto?> CheckInAttendanceSlotAsync(int teacherId, int classId, TeacherAttendanceCheckInRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<AttendanceRecordDto>?> GetAttendanceByDateAsync(int teacherId, int classId, DateOnly attendanceDate, int scheduleId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<AttendanceRecordDto>?> UpsertAttendanceAsync(int teacherId, int classId, UpsertAttendanceRequest request, CancellationToken cancellationToken = default);

    Task<AttendanceSummaryDto?> GetAttendanceSummaryAsync(int teacherId, int classId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TeacherGradeComponentDto>?> GetGradeComponentsAsync(int teacherId, int classId, CancellationToken cancellationToken = default);

    Task<TeacherGradeComponentDto?> CreateGradeComponentAsync(int teacherId, int classId, UpsertGradeComponentRequest request, CancellationToken cancellationToken = default);

    Task<TeacherGradeComponentDto?> UpdateGradeComponentAsync(int teacherId, int classId, int componentId, UpsertGradeComponentRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteGradeComponentAsync(int teacherId, int classId, int componentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<GradeEntryDto>?> GetGradesAsync(int teacherId, int classId, int componentId, CancellationToken cancellationToken = default);

    Task<GradeEntryDto?> UpsertGradeAsync(int teacherId, int classId, int componentId, UpsertGradeRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ApplicationDto>> GetApplicationsAsync(int teacherId, CancellationToken cancellationToken = default);

    Task<ApplicationDto> CreateApplicationAsync(int teacherId, CreateTeacherApplicationRequest request, CancellationToken cancellationToken = default);
}