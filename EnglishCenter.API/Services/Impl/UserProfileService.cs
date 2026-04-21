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
