using System.Text.RegularExpressions;
using EnglishCenter.API.DTOs.Users;
using EnglishCenter.API.Models;
using EnglishCenter.API.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.API.Services.Impl;

public sealed class UserProfileService : IUserProfileService
{
    private readonly EnglishCenterDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserProfileService(EnglishCenterDbContext dbContext, IPasswordHasher<User> passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserProfileDto?> GetProfileAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        return user is null ? null : MapToDto(user);
    }

    public async Task<(bool Success, string? Error)> UpdateProfileAsync(int userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var profileError = ValidateProfileRequest(request);
        if (profileError is not null)
        {
            return (false, profileError);
        }

        var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.UserId == userId, cancellationToken);
        if (user is null)
        {
            return (false, "User not found.");
        }

        user.Fullname = request.Fullname.Trim();
        user.Gender = string.IsNullOrWhiteSpace(request.Gender) ? null : request.Gender.Trim();
        user.Dob = request.Dob;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return (false, "Current and new password are required.");
        }

        if (!string.Equals(request.NewPassword, request.ConfirmPassword, StringComparison.Ordinal))
        {
            return (false, "Confirm password does not match new password.");
        }

        if (string.Equals(request.CurrentPassword, request.NewPassword, StringComparison.Ordinal))
        {
            return (false, "New password must be different from current password.");
        }

        var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.UserId == userId, cancellationToken);
        if (user is null)
        {
            return (false, "User not found.");
        }

        var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
        if (verifyResult == PasswordVerificationResult.Failed)
        {
            return (false, "Current password is incorrect.");
        }

        var passwordError = ValidatePasswordComplexity(request.NewPassword);
        if (passwordError is not null)
        {
            return (false, passwordError);
        }

        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    private static string? ValidatePasswordComplexity(string password)
    {
        if (password.Length < 8)
            return "Password must be at least 8 characters.";
        if (!password.Any(char.IsUpper))
            return "Password must contain at least one uppercase letter.";
        if (!password.Any(char.IsLower))
            return "Password must contain at least one lowercase letter.";
        if (!password.Any(char.IsDigit))
            return "Password must contain at least one digit.";
        if (!Regex.IsMatch(password, @"[^a-zA-Z0-9]"))
            return "Password must contain at least one special character.";
        return null;
    }

    private static string? ValidateProfileRequest(UpdateProfileRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Fullname))
        {
            return "Fullname is required.";
        }

        if (request.Fullname.Trim().Length > 100)
        {
            return "Fullname cannot exceed 100 characters.";
        }

        if (!string.IsNullOrWhiteSpace(request.Gender))
        {
            var gender = request.Gender.Trim();
            if (gender.Length > 10)
            {
                return "Gender cannot exceed 10 characters.";
            }

            if (!string.Equals(gender, "Male", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(gender, "Female", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(gender, "Other", StringComparison.OrdinalIgnoreCase))
            {
                return "Gender must be one of: Male, Female, Other.";
            }
        }

        if (request.Dob.HasValue && request.Dob.Value > DateOnly.FromDateTime(DateTime.Today))
        {
            return "Date of birth cannot be in the future.";
        }

        return null;
    }

    private static UserProfileDto MapToDto(User user) => new()
    {
        UserId = user.UserId,
        Username = user.Username,
        Fullname = user.Fullname,
        Email = user.Email,
        Gender = user.Gender,
        Dob = user.Dob,
        Role = user.Role
    };
}
