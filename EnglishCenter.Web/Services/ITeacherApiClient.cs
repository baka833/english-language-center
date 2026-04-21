using EnglishCenter.Web.Models.Teacher;

namespace EnglishCenter.Web.Services;

public interface ITeacherApiClient
{
    Task<IReadOnlyCollection<TeacherAssignedClassItem>> GetAssignedClassesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TeacherScheduleItem>> GetScheduleAsync(DateOnly? fromDate, DateOnly? toDate, int? month, int? year, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TeacherStudentItem>?> GetStudentsByClassAsync(int classId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<AttendanceSlotItem>?> GetAttendanceSlotsAsync(int classId, DateOnly attendanceDate, CancellationToken cancellationToken = default);

    Task<AttendanceSlotItem?> CheckInAttendanceSlotAsync(int classId, TeacherAttendanceCheckInRequestModel request, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<AttendanceRecordItem>?> GetAttendanceByDateAsync(int classId, DateOnly attendanceDate, int scheduleId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<AttendanceRecordItem>?> UpsertAttendanceAsync(int classId, UpsertAttendanceRequestModel request, CancellationToken cancellationToken = default);

    Task<AttendanceSummaryItem?> GetAttendanceSummaryAsync(int classId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TeacherGradeComponentItem>?> GetGradeComponentsAsync(int classId, CancellationToken cancellationToken = default);

    Task<TeacherGradeComponentItem?> CreateGradeComponentAsync(int classId, UpsertGradeComponentRequestModel request, CancellationToken cancellationToken = default);

    Task<TeacherGradeComponentItem?> UpdateGradeComponentAsync(int classId, int componentId, UpsertGradeComponentRequestModel request, CancellationToken cancellationToken = default);

    Task<bool> DeleteGradeComponentAsync(int classId, int componentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<GradeEntryItem>?> GetGradesAsync(int classId, int componentId, CancellationToken cancellationToken = default);

    Task<GradeEntryItem?> UpsertGradeAsync(int classId, int componentId, UpsertGradeRequestModel request, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TeacherApplicationItem>> GetApplicationsAsync(CancellationToken cancellationToken = default);

    Task<TeacherApplicationItem> CreateApplicationAsync(CreateTeacherApplicationRequestModel request, CancellationToken cancellationToken = default);
}
