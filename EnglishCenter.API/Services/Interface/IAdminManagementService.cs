using EnglishCenter.API.DTOs.Admin;

namespace EnglishCenter.API.Services.Interface;

public interface IAdminManagementService
{
    Task<IReadOnlyCollection<UserSummaryDto>> GetUsersAsync(string? role, bool? isActive, CancellationToken cancellationToken = default);

    Task<UserDetailDto?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<UserDetailDto> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);

    Task<UserDetailDto?> UpdateUserAsync(int userId, UpdateUserRequest request, CancellationToken cancellationToken = default);

    Task<UserDetailDto?> SetUserActivationAsync(int userId, bool isActive, CancellationToken cancellationToken = default);

    Task<UserDetailDto?> UpdateTeacherGradeComponentPermissionAsync(int userId, bool canManageGradeComponents, CancellationToken cancellationToken = default);

    Task<ResetPasswordResultDto?> ResetPasswordAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CourseDto>> GetCoursesAsync(CancellationToken cancellationToken = default);

    Task<CourseDto?> GetCourseByIdAsync(int courseId, CancellationToken cancellationToken = default);

    Task<CourseDto> CreateCourseAsync(UpsertCourseRequest request, CancellationToken cancellationToken = default);

    Task<CourseDto?> UpdateCourseAsync(int courseId, UpsertCourseRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteCourseAsync(int courseId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ClassListDto>> GetClassesAsync(CancellationToken cancellationToken = default);

    Task<ClassDetailDto?> GetClassByIdAsync(int classId, CancellationToken cancellationToken = default);

    Task<ClassDetailDto> CreateClassAsync(CreateClassRequest request, CancellationToken cancellationToken = default);

    Task<ClassDetailDto?> UpdateClassAsync(int classId, UpdateClassRequest request, CancellationToken cancellationToken = default);

    Task<ClassDetailDto?> UpdateClassGradeComponentPermissionAsync(int classId, bool allowTeacherGradeComponentManagement, CancellationToken cancellationToken = default);

    Task<bool> DeleteClassAsync(int classId, CancellationToken cancellationToken = default);

    Task<ClassDetailDto?> AssignTeacherAsync(int classId, int teacherId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ClassStudentDto>?> AssignStudentsAsync(int classId, IReadOnlyCollection<int> studentIds, CancellationToken cancellationToken = default);

    Task<bool> RemoveStudentAsync(int classId, int studentId, CancellationToken cancellationToken = default);

    Task<ClassSchedulePlannerDto?> GetClassSchedulePlannerAsync(int classId, int? month, int? year, int? weekIndex, CancellationToken cancellationToken = default);

    Task<ClassSchedulePlannerDto?> SaveClassSchedulePlannerAsync(int classId, SaveClassSchedulePlannerRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ScheduleDto>?> UpdateClassSchedulesAsync(int classId, IReadOnlyCollection<UpsertScheduleRequest> schedules, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ApplicationDto>> GetApplicationsAsync(string? status, CancellationToken cancellationToken = default);

    Task<ApplicationDto?> RespondApplicationAsync(int applicationId, RespondApplicationRequest request, CancellationToken cancellationToken = default);
}