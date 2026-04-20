using EnglishCenter.Web.Models.Teacher;

namespace EnglishCenter.Web.Services;

public interface ITeacherApiClient
{
    Task<IReadOnlyCollection<TeacherAssignedClassItem>> GetAssignedClassesAsync(int teacherId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TeacherScheduleItem>> GetScheduleAsync(int teacherId, DateOnly? fromDate, DateOnly? toDate, int? month, int? year, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TeacherStudentItem>?> GetStudentsByClassAsync(int teacherId, int classId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<AttendanceRecordItem>?> GetAttendanceByDateAsync(int teacherId, int classId, DateOnly attendanceDate, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<AttendanceRecordItem>?> UpsertAttendanceAsync(int teacherId, int classId, UpsertAttendanceRequestModel request, CancellationToken cancellationToken = default);

    Task<AttendanceSummaryItem?> GetAttendanceSummaryAsync(int teacherId, int classId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TeacherGradeComponentItem>?> GetGradeComponentsAsync(int teacherId, int classId, CancellationToken cancellationToken = default);

    Task<TeacherGradeComponentItem?> CreateGradeComponentAsync(int teacherId, int classId, UpsertGradeComponentRequestModel request, CancellationToken cancellationToken = default);

    Task<TeacherGradeComponentItem?> UpdateGradeComponentAsync(int teacherId, int classId, int componentId, UpsertGradeComponentRequestModel request, CancellationToken cancellationToken = default);

    Task<bool> DeleteGradeComponentAsync(int teacherId, int classId, int componentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<GradeEntryItem>?> GetGradesAsync(int teacherId, int classId, int componentId, CancellationToken cancellationToken = default);

    Task<GradeEntryItem?> UpsertGradeAsync(int teacherId, int classId, int componentId, UpsertGradeRequestModel request, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TeacherApplicationItem>> GetApplicationsAsync(int teacherId, CancellationToken cancellationToken = default);

    Task<TeacherApplicationItem> CreateApplicationAsync(int teacherId, CreateTeacherApplicationRequestModel request, CancellationToken cancellationToken = default);
}
