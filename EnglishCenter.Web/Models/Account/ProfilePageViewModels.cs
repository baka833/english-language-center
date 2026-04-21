using System.ComponentModel.DataAnnotations;

namespace EnglishCenter.Web.Models.Account;

public sealed class ProfileViewModel
{
    public UserProfileApiModel Profile { get; set; } = new();
}

public sealed class EditProfileViewModel
{
    public string Username { get; set; } = string.Empty;
    public string? Role { get; set; }

    [Required(ErrorMessage = "Full name is required.")]
    public string Fullname { get; set; } = string.Empty;

    public string? Gender { get; set; }
    public DateOnly? Dob { get; set; }
}

public sealed class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Current password is required.")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required."), MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your new password."), Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
