using System.Net.Http.Json;
using System.Text.Json;
using EnglishCenter.Web.Models.Auth;

namespace EnglishCenter.Web.Services;

public sealed class AuthApiClient : IAuthApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;

    public AuthApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AuthResponseModel> LoginAsync(LoginForm form, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/auth/login", form, JsonOptions, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var auth = await response.Content.ReadFromJsonAsync<AuthResponseModel>(JsonOptions, cancellationToken);
            if (auth is null)
            {
                throw new InvalidOperationException("API returned an empty authentication response.");
            }

            return auth;
        }

        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException(TryReadMessage(payload) ?? $"Authentication failed with status {(int)response.StatusCode}.");
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
