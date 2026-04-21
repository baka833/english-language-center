using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EnglishCenter.Web.Models.Account;

namespace EnglishCenter.Web.Services;

public sealed class UserApiClient : IUserApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;

    public UserApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<UserProfileApiModel?> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync("api/users/profile", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response, cancellationToken);
        return await ReadAsync<UserProfileApiModel>(response, cancellationToken);
    }

    public async Task<(bool Success, string? Error)> UpdateProfileAsync(UpdateProfileApiRequest request, CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Put, "api/users/profile")
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        };

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        return (false, TryReadMessage(payload) ?? $"API request failed with status {(int)response.StatusCode}.");
    }

    public async Task<(bool Success, string? Error)> ChangePasswordAsync(ChangePasswordApiRequest request, CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/users/change-password")
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        };

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        return (false, TryReadMessage(payload) ?? $"API request failed with status {(int)response.StatusCode}.");
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

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        if (result is null)
        {
            throw new InvalidOperationException("API returned an empty response.");
        }

        return result;
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
