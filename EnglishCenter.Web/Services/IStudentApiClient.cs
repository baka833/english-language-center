using EnglishCenter.Web.Models.Student;

namespace EnglishCenter.Web.Services;

public interface IStudentApiClient
{
    Task<IReadOnlyCollection<StudentScheduleItem>> GetScheduleAsync(int studentId, DateOnly? fromDate, DateOnly? toDate, int? month, int? year, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<StudentApplicationItem>> GetApplicationsAsync(int studentId, CancellationToken cancellationToken = default);

    Task<StudentApplicationItem> CreateApplicationAsync(int studentId, CreateStudentApplicationRequestModel request, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<StudentClassItem>> GetClassesAsync(int studentId, CancellationToken cancellationToken = default);

    Task<StudentClassDetailItem> GetClassDetailAsync(int studentId, int classId, CancellationToken cancellationToken = default);
}
