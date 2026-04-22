using EnglishCenter.Web.Models.Admin;

namespace EnglishCenter.Web.Services.Interface;

public interface IAdminApiClient
{
    Task<IReadOnlyCollection<AdminUserSummaryItem>> GetUsersAsync(string? role, bool? isActive, CancellationToken cancellationToken = default);

    Task<AdminUserDetailItem?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);

    Task CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);

    Task<AdminUserDetailItem?> UpdateUserAsync(int userId, UpdateUserRequest request, CancellationToken cancellationToken = default);

    Task<AdminUserDetailItem?> SetUserActivationAsync(int userId, bool isActive, CancellationToken cancellationToken = default);

    Task<AdminUserDetailItem?> SetTeacherGradePermissionAsync(int userId, bool canManageGradeComponents, CancellationToken cancellationToken = default);

    Task<ResetPasswordResultItem?> ResetPasswordAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CourseItem>> GetCoursesAsync(CancellationToken cancellationToken = default);

    Task<CourseItem?> GetCourseByIdAsync(int courseId, CancellationToken cancellationToken = default);

    Task CreateCourseAsync(UpsertCourseRequest request, CancellationToken cancellationToken = default);

    Task<CourseItem?> UpdateCourseAsync(int courseId, UpsertCourseRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteCourseAsync(int courseId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ClassListItem>> GetClassesAsync(CancellationToken cancellationToken = default);

    Task<ClassDetailItem?> GetClassByIdAsync(int classId, CancellationToken cancellationToken = default);

    Task CreateClassAsync(CreateClassRequest request, CancellationToken cancellationToken = default);

    Task<ClassDetailItem?> UpdateClassAsync(int classId, UpdateClassRequest request, CancellationToken cancellationToken = default);

    Task<ClassDetailItem?> SetClassGradePermissionAsync(int classId, bool allowTeacherGradeComponentManagement, CancellationToken cancellationToken = default);

    Task<bool> DeleteClassAsync(int classId, CancellationToken cancellationToken = default);

    Task<ClassDetailItem?> AssignTeacherAsync(int classId, int teacherId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ClassStudentItem>?> AssignStudentsAsync(int classId, IReadOnlyCollection<int> studentIds, CancellationToken cancellationToken = default);

    Task<bool> RemoveStudentAsync(int classId, int studentId, CancellationToken cancellationToken = default);

    Task<ClassSchedulePlannerPageViewModel?> GetSchedulePlannerAsync(int classId, int? month, int? year, int? weekIndex, CancellationToken cancellationToken = default);

    Task<ClassSchedulePlannerPageViewModel?> SaveSchedulePlannerAsync(int classId, SaveSchedulePlannerForm request, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ScheduleItem>?> UpdateSchedulesAsync(int classId, IReadOnlyCollection<UpsertScheduleRequest> schedules, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ApplicationItem>> GetApplicationsAsync(string? status, CancellationToken cancellationToken = default);

    Task<ApplicationItem?> RespondApplicationAsync(int applicationId, RespondApplicationRequest request, CancellationToken cancellationToken = default);
}