using EnglishCenter.Web.Models.Account;

namespace EnglishCenter.Web.Services.Interface;

public interface IUserApiClient
{
    Task<UserProfileApiModel?> GetProfileAsync(CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> UpdateProfileAsync(UpdateProfileApiRequest request, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> ChangePasswordAsync(ChangePasswordApiRequest request, CancellationToken cancellationToken = default);
}
