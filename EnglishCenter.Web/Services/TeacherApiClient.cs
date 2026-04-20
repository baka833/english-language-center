using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EnglishCenter.Web.Models.Teacher;

namespace EnglishCenter.Web.Services;

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

    public Task<IReadOnlyCollection<TeacherAssignedClassItem>> GetAssignedClassesAsync(int teacherId, CancellationToken cancellationToken = default)
    {
        return GetRequiredAsync<IReadOnlyCollection<TeacherAssignedClassItem>>($"api/teachers/{teacherId}/classes", cancellationToken);
    }

    public Task<IReadOnlyCollection<TeacherScheduleItem>> GetScheduleAsync(int teacherId, DateOnly? fromDate, DateOnly? toDate, int? month, int? year, CancellationToken cancellationToken = default)
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
            ? $"api/teachers/{teacherId}/schedule"
            : $"api/teachers/{teacherId}/schedule?{string.Join("&", queryParts)}";

        return GetRequiredAsync<IReadOnlyCollection<TeacherScheduleItem>>(path, cancellationToken);
    }

    public Task<IReadOnlyCollection<TeacherStudentItem>?> GetStudentsByClassAsync(int teacherId, int classId, CancellationToken cancellationToken = default)
    {
        return GetOptionalAsync<IReadOnlyCollection<TeacherStudentItem>>($"api/teachers/{teacherId}/classes/{classId}/students", cancellationToken);
    }

    public Task<IReadOnlyCollection<AttendanceRecordItem>?> GetAttendanceByDateAsync(int teacherId, int classId, DateOnly attendanceDate, CancellationToken cancellationToken = default)
    {
        return GetOptionalAsync<IReadOnlyCollection<AttendanceRecordItem>>($"api/teachers/{teacherId}/classes/{classId}/attendance?attendanceDate={attendanceDate:yyyy-MM-dd}", cancellationToken);
    }

    public Task<IReadOnlyCollection<AttendanceRecordItem>?> UpsertAttendanceAsync(int teacherId, int classId, UpsertAttendanceRequestModel request, CancellationToken cancellationToken = default)
    {
        return SendOptionalAsync<IReadOnlyCollection<AttendanceRecordItem>>(HttpMethod.Put, $"api/teachers/{teacherId}/classes/{classId}/attendance", request, cancellationToken);
    }

    public Task<AttendanceSummaryItem?> GetAttendanceSummaryAsync(int teacherId, int classId, CancellationToken cancellationToken = default)
    {
        return GetOptionalAsync<AttendanceSummaryItem>($"api/teachers/{teacherId}/classes/{classId}/attendance-summary", cancellationToken);
    }

    public Task<IReadOnlyCollection<TeacherGradeComponentItem>?> GetGradeComponentsAsync(int teacherId, int classId, CancellationToken cancellationToken = default)
    {
        return GetOptionalAsync<IReadOnlyCollection<TeacherGradeComponentItem>>($"api/teachers/{teacherId}/classes/{classId}/grade-components", cancellationToken);
    }

    public Task<TeacherGradeComponentItem?> CreateGradeComponentAsync(int teacherId, int classId, UpsertGradeComponentRequestModel request, CancellationToken cancellationToken = default)
    {
        return SendOptionalAsync<TeacherGradeComponentItem>(HttpMethod.Post, $"api/teachers/{teacherId}/classes/{classId}/grade-components", request, cancellationToken);
    }

    public Task<TeacherGradeComponentItem?> UpdateGradeComponentAsync(int teacherId, int classId, int componentId, UpsertGradeComponentRequestModel request, CancellationToken cancellationToken = default)
    {
        return SendOptionalAsync<TeacherGradeComponentItem>(HttpMethod.Put, $"api/teachers/{teacherId}/classes/{classId}/grade-components/{componentId}", request, cancellationToken);
    }

    public async Task<bool> DeleteGradeComponentAsync(int teacherId, int classId, int componentId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"api/teachers/{teacherId}/classes/{classId}/grade-components/{componentId}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        await EnsureSuccessAsync(response, cancellationToken);
        return true;
    }

    public Task<IReadOnlyCollection<GradeEntryItem>?> GetGradesAsync(int teacherId, int classId, int componentId, CancellationToken cancellationToken = default)
    {
        return GetOptionalAsync<IReadOnlyCollection<GradeEntryItem>>($"api/teachers/{teacherId}/classes/{classId}/grade-components/{componentId}/grades", cancellationToken);
    }

    public Task<GradeEntryItem?> UpsertGradeAsync(int teacherId, int classId, int componentId, UpsertGradeRequestModel request, CancellationToken cancellationToken = default)
    {
        return SendOptionalAsync<GradeEntryItem>(HttpMethod.Put, $"api/teachers/{teacherId}/classes/{classId}/grade-components/{componentId}/grades", request, cancellationToken);
    }

    public Task<IReadOnlyCollection<TeacherApplicationItem>> GetApplicationsAsync(int teacherId, CancellationToken cancellationToken = default)
    {
        return GetRequiredAsync<IReadOnlyCollection<TeacherApplicationItem>>($"api/teachers/{teacherId}/applications", cancellationToken);
    }

    public Task<TeacherApplicationItem> CreateApplicationAsync(int teacherId, CreateTeacherApplicationRequestModel request, CancellationToken cancellationToken = default)
    {
        return SendRequiredAsync<TeacherApplicationItem>(HttpMethod.Post, $"api/teachers/{teacherId}/applications", request, cancellationToken);
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
