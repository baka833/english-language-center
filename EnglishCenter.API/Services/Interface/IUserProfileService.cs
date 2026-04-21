using EnglishCenter.API.DTOs.Users;

namespace EnglishCenter.API.Services.Interface;

public interface IUserProfileService
{
    Task<UserProfileDto?> GetProfileAsync(int userId, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> UpdateProfileAsync(int userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
}
