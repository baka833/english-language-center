using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EnglishCenter.Web.Models.Teacher;
using EnglishCenter.Web.Services.Interface;

namespace EnglishCenter.Web.Services.Impl;

public sealed class TeacherApiClient : ITeacherApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;

    public TeacherApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<IReadOnlyCollection<TeacherAssignedClassItem>> GetAssignedClassesAsync(CancellationToken cancellationToken = default)
    {
        return GetRequiredAsync<IReadOnlyCollection<TeacherAssignedClassItem>>("api/teacher/classes", cancellationToken);
    }

    public Task<IReadOnlyCollection<TeacherScheduleItem>> GetScheduleAsync(DateOnly? fromDate, DateOnly? toDate, int? month, int? year, CancellationToken cancellationToken = default)
    {
        var queryParts = new List<string>();
        if (fromDate.HasValue)
        {
            queryParts.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
        }

        if (toDate.HasValue)
        {
            queryParts.Add($"toDate={toDate.Value:yyyy-MM-dd}");
        }

        if (month.HasValue)
        {
            queryParts.Add($"month={month.Value}");
        }

        if (year.HasValue)
        {
            queryParts.Add($"year={year.Value}");
        }

        var path = queryParts.Count == 0
            ? "api/teacher/schedule"
            : $"api/teacher/schedule?{string.Join("&", queryParts)}";

        return GetRequiredAsync<IReadOnlyCollection<TeacherScheduleItem>>(path, cancellationToken);
    }

    public Task<IReadOnlyCollection<TeacherStudentItem>?> GetStudentsByClassAsync(int classId, CancellationToken cancellationToken = default)
    {
        return GetOptionalAsync<IReadOnlyCollection<TeacherStudentItem>>($"api/teacher/classes/{classId}/students", cancellationToken);
    }

    public Task<IReadOnlyCollection<AttendanceSlotItem>?> GetAttendanceSlotsAsync(int classId, DateOnly attendanceDate, CancellationToken cancellationToken = default)
    {
        return GetOptionalAsync<IReadOnlyCollection<AttendanceSlotItem>>($"api/teacher/classes/{classId}/attendance-slots?attendanceDate={attendanceDate:yyyy-MM-dd}", cancellationToken);
    }

    public Task<AttendanceSlotItem?> CheckInAttendanceSlotAsync(int classId, TeacherAttendanceCheckInRequestModel request, CancellationToken cancellationToken = default)
    {
        return SendOptionalAsync<AttendanceSlotItem>(HttpMethod.Post, $"api/teacher/classes/{classId}/attendance-checkin", request, cancellationToken);
    }

    public Task<IReadOnlyCollection<AttendanceRecordItem>?> GetAttendanceByDateAsync(int classId, DateOnly attendanceDate, int scheduleId, CancellationToken cancellationToken = default)
    {
        return GetOptionalAsync<IReadOnlyCollection<AttendanceRecordItem>>($"api/teacher/classes/{classId}/attendance?attendanceDate={attendanceDate:yyyy-MM-dd}&scheduleId={scheduleId}", cancellationToken);
    }

    public Task<IReadOnlyCollection<AttendanceRecordItem>?> UpsertAttendanceAsync(int classId, UpsertAttendanceRequestModel request, CancellationToken cancellationToken = default)
    {
        return SendOptionalAsync<IReadOnlyCollection<AttendanceRecordItem>>(HttpMethod.Put, $"api/teacher/classes/{classId}/attendance", request, cancellationToken);
    }

    public Task<AttendanceSummaryItem?> GetAttendanceSummaryAsync(int classId, CancellationToken cancellationToken = default)
    {
        return GetOptionalAsync<AttendanceSummaryItem>($"api/teacher/classes/{classId}/attendance-summary", cancellationToken);
    }

    public Task<IReadOnlyCollection<TeacherGradeComponentItem>?> GetGradeComponentsAsync(int classId, CancellationToken cancellationToken = default)
    {
        return GetOptionalAsync<IReadOnlyCollection<TeacherGradeComponentItem>>($"api/teacher/classes/{classId}/grade-components", cancellationToken);
    }

    public Task<TeacherGradeComponentItem?> CreateGradeComponentAsync(int classId, UpsertGradeComponentRequestModel request, CancellationToken cancellationToken = default)
    {
        return SendOptionalAsync<TeacherGradeComponentItem>(HttpMethod.Post, $"api/teacher/classes/{classId}/grade-components", request, cancellationToken);
    }

    public Task<TeacherGradeComponentItem?> UpdateGradeComponentAsync(int classId, int componentId, UpsertGradeComponentRequestModel request, CancellationToken cancellationToken = default)
    {
        return SendOptionalAsync<TeacherGradeComponentItem>(HttpMethod.Put, $"api/teacher/classes/{classId}/grade-components/{componentId}", request, cancellationToken);
    }

    public async Task<bool> DeleteGradeComponentAsync(int classId, int componentId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"api/teacher/classes/{classId}/grade-components/{componentId}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        await EnsureSuccessAsync(response, cancellationToken);
        return true;
    }

    public Task<IReadOnlyCollection<GradeEntryItem>?> GetGradesAsync(int classId, int componentId, CancellationToken cancellationToken = default)
    {
        return GetOptionalAsync<IReadOnlyCollection<GradeEntryItem>>($"api/teacher/classes/{classId}/grade-components/{componentId}/grades", cancellationToken);
    }

    public Task<GradeEntryItem?> UpsertGradeAsync(int classId, int componentId, UpsertGradeRequestModel request, CancellationToken cancellationToken = default)
    {
        return SendOptionalAsync<GradeEntryItem>(HttpMethod.Put, $"api/teacher/classes/{classId}/grade-components/{componentId}/grades", request, cancellationToken);
    }

    public Task<IReadOnlyCollection<TeacherApplicationItem>> GetApplicationsAsync(CancellationToken cancellationToken = default)
    {
        return GetRequiredAsync<IReadOnlyCollection<TeacherApplicationItem>>("api/teacher/applications", cancellationToken);
    }

    public Task<TeacherApplicationItem> CreateApplicationAsync(CreateTeacherApplicationRequestModel request, CancellationToken cancellationToken = default)
    {
        return SendRequiredAsync<TeacherApplicationItem>(HttpMethod.Post, "api/teacher/applications", request, cancellationToken);
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

    private async Task<T> SendRequiredAsync<T>(HttpMethod method, string path, object payload, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, path)
        {
            Content = JsonContent.Create(payload, options: JsonOptions)
        };

        using var response = await _httpClient.SendAsync(request, cancellationToken);
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
