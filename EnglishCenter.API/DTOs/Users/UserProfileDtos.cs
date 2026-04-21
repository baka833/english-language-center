using System.ComponentModel.DataAnnotations;

namespace EnglishCenter.API.DTOs.Users;

public class UserProfileDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
    public string Fullname { get; set; } = null!;
    public string? Email { get; set; }
    public string? Gender { get; set; }
    public DateOnly? Dob { get; set; }
    public string? Role { get; set; }
}

public class UpdateProfileRequest
{
    [Required]
    public string Fullname { get; set; } = null!;
    public string? Gender { get; set; }
    public DateOnly? Dob { get; set; }
}

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = null!;

    [Required]
    public string NewPassword { get; set; } = null!;

    [Required, Compare(nameof(NewPassword))]
    public string ConfirmPassword { get; set; } = null!;
}
