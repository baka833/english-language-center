using System.Net.Http.Json;
using System.Text.Json;
using EnglishCenter.Web.Models.Student;

namespace EnglishCenter.Web.Services;

public sealed class StudentApiClient : IStudentApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;

    public StudentApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<IReadOnlyCollection<StudentScheduleItem>> GetScheduleAsync(int studentId, DateOnly? fromDate, DateOnly? toDate, int? month, int? year, CancellationToken cancellationToken = default)
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
            ? $"api/students/{studentId}/schedule"
            : $"api/students/{studentId}/schedule?{string.Join("&", queryParts)}";

        return GetRequiredAsync<IReadOnlyCollection<StudentScheduleItem>>(path, cancellationToken);
    }

    public Task<IReadOnlyCollection<StudentApplicationItem>> GetApplicationsAsync(int studentId, CancellationToken cancellationToken = default)
    {
        return GetRequiredAsync<IReadOnlyCollection<StudentApplicationItem>>($"api/students/{studentId}/applications", cancellationToken);
    }

    public Task<StudentApplicationItem> CreateApplicationAsync(int studentId, CreateStudentApplicationRequestModel request, CancellationToken cancellationToken = default)
    {
        return SendRequiredAsync<StudentApplicationItem>(HttpMethod.Post, $"api/students/{studentId}/applications", request, cancellationToken);
    }

    private async Task<T> GetRequiredAsync<T>(string path, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(path, cancellationToken);
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
