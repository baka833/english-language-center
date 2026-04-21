using EnglishCenter.API.DTOs;

namespace EnglishCenter.API.Services.Interface;

public interface IStudentManagementService
{
    Task<IReadOnlyCollection<StudentScheduleItemDto>> GetScheduleAsync(int studentId, DateOnly? fromDate, DateOnly? toDate, int? month, int? year, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ApplicationDto>> GetApplicationsAsync(int studentId, CancellationToken cancellationToken = default);

    Task<ApplicationDto> CreateApplicationAsync(int studentId, CreateStudentApplicationRequest request, CancellationToken cancellationToken = default);
}
