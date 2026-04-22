using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EnglishCenter.Web.Models.Admin;
using EnglishCenter.Web.Services.Interface;

namespace EnglishCenter.Web.Services.Impl;

public sealed class AdminApiClient : IAdminApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;

    public AdminApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<IReadOnlyCollection<AdminUserSummaryItem>> GetUsersAsync(string? role, bool? isActive, CancellationToken cancellationToken = default)
    {
        var queryParts = new List<string>();

        if (!string.IsNullOrWhiteSpace(role))
        {
            queryParts.Add($"role={Uri.EscapeDataString(role)}");
        }

        if (isActive.HasValue)
        {
            queryParts.Add($"isActive={isActive.Value.ToString().ToLowerInvariant()}");
        }

        var path = queryParts.Count == 0 ? "api/admin/users" : $"api/admin/users?{string.Join("&", queryParts)}";
        return GetRequiredAsync<IReadOnlyCollection<AdminUserSummaryItem>>(path, cancellationToken);
    }

    public Task<AdminUserDetailItem?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return GetOptionalAsync<AdminUserDetailItem>($"api/admin/users/{userId}", cancellationToken);
    }

    public async Task CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/admin/users", request, JsonOptions, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public Task<AdminUserDetailItem?> UpdateUserAsync(int userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        return SendOptionalAsync<AdminUserDetailItem>(HttpMethod.Put, $"api/admin/users/{userId}", request, cancellationToken);
    }

    public Task<AdminUserDetailItem?> SetUserActivationAsync(int userId, bool isActive, CancellationToken cancellationToken = default)
    {
        return SendOptionalAsync<AdminUserDetailItem>(new HttpMethod("PATCH"), $"api/admin/users/{userId}/activation", new LockUserRequest { IsActive = isActive }, cancellationToken);
    }

    public Task<AdminUserDetailItem?> SetTeacherGradePermissionAsync(int userId, bool canManageGradeComponents, CancellationToken cancellationToken = default)
    {
        return SendOptionalAsync<AdminUserDetailItem>(new HttpMethod("PATCH"), $"api/admin/users/{userId}/grade-component-permission", new UpdateTeacherGradeComponentPermissionRequest { CanManageGradeComponents = canManageGradeComponents }, cancellationToken);
    }

    public Task<ResetPasswordResultItem?> ResetPasswordAsync(int userId, CancellationToken cancellationToken = default)
    {
        return SendOptionalAsync<ResetPasswordResultItem>(HttpMethod.Post, $"api/admin/users/{userId}/reset-password", new { }, cancellationToken);
    }

    public Task<IReadOnlyCollection<CourseItem>> GetCoursesAsync(CancellationToken cancellationToken = default)
    {
        return GetRequiredAsync<IReadOnlyCollection<CourseItem>>("api/admin/courses", cancellationToken);
    }

    public Task<CourseItem?> GetCourseByIdAsync(int courseId, CancellationToken cancellationToken = default)
    {
        return GetOptionalAsync<CourseItem>($"api/admin/courses/{courseId}", cancellationToken);
    }

    public async Task CreateCourseAsync(UpsertCourseRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/admin/courses", request, JsonOptions, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public Task<CourseItem?> UpdateCourseAsync(int courseId, UpsertCourseRequest request, CancellationToken cancellationToken = default)
    {
        return SendOptionalAsync<CourseItem>(HttpMethod.Put, $"api/admin/courses/{courseId}", request, cancellationToken);
    }

    public async Task<bool> DeleteCourseAsync(int courseId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"api/admin/courses/{courseId}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        await EnsureSuccessAsync(response, cancellationToken);
        return true;
    }

    public Task<IReadOnlyCollection<ClassListItem>> GetClassesAsync(CancellationToken cancellationToken = default)
    {
        return GetRequiredAsync<IReadOnlyCollection<ClassListItem>>("api/admin/classes", cancellationToken);
    }

    public Task<ClassDetailItem?> GetClassByIdAsync(int classId, CancellationToken cancellationToken = default)
    {
        return GetOptionalAsync<ClassDetailItem>($"api/admin/classes/{classId}", cancellationToken);
    }

    public async Task CreateClassAsync(CreateClassRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/admin/classes", request, JsonOptions, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public Task<ClassDetailItem?> UpdateClassAsync(int classId, UpdateClassRequest request, CancellationToken cancellationToken = default)
    {
        return SendOptionalAsync<ClassDetailItem>(HttpMethod.Put, $"api/admin/classes/{classId}", request, cancellationToken);
    }

    public Task<ClassDetailItem?> SetClassGradePermissionAsync(int classId, bool allowTeacherGradeComponentManagement, CancellationToken cancellationToken = default)
    {
        return SendOptionalAsync<ClassDetailItem>(new HttpMethod("PATCH"), $"api/admin/classes/{classId}/grade-component-permission", new UpdateClassGradeComponentPermissionRequest { AllowTeacherGradeComponentManagement = allowTeacherGradeComponentManagement }, cancellationToken);
    }

    public async Task<bool> DeleteClassAsync(int classId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"api/admin/classes/{classId}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        await EnsureSuccessAsync(response, cancellationToken);
        return true;
    }

    public Task<ClassDetailItem?> AssignTeacherAsync(int classId, int teacherId, CancellationToken cancellationToken = default)
    {
        return SendOptionalAsync<ClassDetailItem>(HttpMethod.Put, $"api/admin/classes/{classId}/teacher", new AssignTeacherRequest { TeacherId = teacherId }, cancellationToken);
    }

    public Task<IReadOnlyCollection<ClassStudentItem>?> AssignStudentsAsync(int classId, IReadOnlyCollection<int> studentIds, CancellationToken cancellationToken = default)
    {
        return SendOptionalAsync<IReadOnlyCollection<ClassStudentItem>>(HttpMethod.Post, $"api/admin/classes/{classId}/students", new AssignStudentsRequest { StudentIds = studentIds }, cancellationToken);
    }

    public async Task<bool> RemoveStudentAsync(int classId, int studentId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"api/admin/classes/{classId}/students/{studentId}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        await EnsureSuccessAsync(response, cancellationToken);
        return true;
    }

    public Task<ClassSchedulePlannerPageViewModel?> GetSchedulePlannerAsync(int classId, int? month, int? year, int? weekIndex, CancellationToken cancellationToken = default)
    {
        var queryParts = new List<string>();
        if (month.HasValue)
        {
            queryParts.Add($"month={month.Value}");
        }

        if (year.HasValue)
        {
            queryParts.Add($"year={year.Value}");
        }

        if (weekIndex.HasValue)
        {
            queryParts.Add($"weekIndex={weekIndex.Value}");
        }

        var path = queryParts.Count == 0
            ? $"api/admin/classes/{classId}/schedule-planner"
            : $"api/admin/classes/{classId}/schedule-planner?{string.Join("&", queryParts)}";

        return GetOptionalAsync<ClassSchedulePlannerPageViewModel>(path, cancellationToken);
    }

    public Task<ClassSchedulePlannerPageViewModel?> SaveSchedulePlannerAsync(int classId, SaveSchedulePlannerForm request, CancellationToken cancellationToken = default)
    {
        return SendOptionalAsync<ClassSchedulePlannerPageViewModel>(HttpMethod.Post, $"api/admin/classes/{classId}/schedule-planner", request, cancellationToken);
    }

    public Task<IReadOnlyCollection<ScheduleItem>?> UpdateSchedulesAsync(int classId, IReadOnlyCollection<UpsertScheduleRequest> schedules, CancellationToken cancellationToken = default)
    {
        return SendOptionalAsync<IReadOnlyCollection<ScheduleItem>>(HttpMethod.Put, $"api/admin/classes/{classId}/schedules", new UpdateClassSchedulesRequest { Schedules = schedules }, cancellationToken);
    }

    public Task<IReadOnlyCollection<ApplicationItem>> GetApplicationsAsync(string? status, CancellationToken cancellationToken = default)
    {
        var path = string.IsNullOrWhiteSpace(status) ? "api/admin/applications" : $"api/admin/applications?status={Uri.EscapeDataString(status)}";
        return GetRequiredAsync<IReadOnlyCollection<ApplicationItem>>(path, cancellationToken);
    }

    public Task<ApplicationItem?> RespondApplicationAsync(int applicationId, RespondApplicationRequest request, CancellationToken cancellationToken = default)
    {
        return SendOptionalAsync<ApplicationItem>(HttpMethod.Post, $"api/admin/applications/{applicationId}/response", request, cancellationToken);
    }

    private async Task<T> GetRequiredAsync<T>(string path, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(path, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await ReadAsync<T>(response, cancellationToken);
    }

    private async Task<T?> GetOptionalAsync<T>(string path, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(path, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return default;
        }

        await EnsureSuccessAsync(response, cancellationToken);
        return await ReadAsync<T>(response, cancellationToken);
    }

    private async Task<T?> SendOptionalAsync<T>(HttpMethod method, string path, object payload, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, path)
        {
            Content = JsonContent.Create(payload, options: JsonOptions)
        };

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return default;
        }

        await EnsureSuccessAsync(response, cancellationToken);
        return await ReadAsync<T>(response, cancellationToken);
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        if (result is null)
        {
            throw new InvalidOperationException("API returned an empty response.");
        }

        return result;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        var message = TryReadMessage(payload) ?? $"API request failed with status {(int)response.StatusCode}.";
        throw new InvalidOperationException(message);
    }

    private static string? TryReadMessage(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return null;
        }

        try
        {
            using var jsonDocument = JsonDocument.Parse(payload);
            if (jsonDocument.RootElement.TryGetProperty("message", out var messageElement))
            {
                return messageElement.GetString();
            }
        }
        catch (JsonException)
        {
            return payload;
        }

        return payload;
    }
}